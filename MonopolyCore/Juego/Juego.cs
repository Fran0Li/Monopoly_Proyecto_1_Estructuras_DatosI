using System.Text;
using MonopolyCore.Comunicacion;
using MonopolyCore.Estructuras;
using MonopolyCore.Modelos;

namespace MonopolyCore
{
    // Orquestador de la partida: ÚNICA fuente de verdad del estado del juego.
    //
    //  El servidor (ServidorTcp) solo recibe mensajes, llama a un método público de esta
    //   clase y reenvía el ResultadoAccion (respuesta + eventos para todos).
    //  Las casillas y cartas aplican su propio comportamiento (polimorfismo) llamando a los
    //   métodos internal de esta clase: OfrecerCompra, CobrarObligatorio, MoverJugador, etc.
    //
    // Estructuras usadas:
    //   Tablero: ListaCircularDoble<Casilla> (movimiento nodo por nodo)
    //   Turnos: ColaCircular<Jugador>
    //   Mazo: ListaCircularDoble<CartaEvento> (la carta usada pasa al final)
    //   Historial: Banco (ListaDoblementeEnlazada<Transaccion>)
    //   Eventos de cada acción: ListaDoblementeEnlazada<EventoJuego>
    public class Juego
    {
        public const int MaxJugadores = 4;

        // Config
        public int SaldoInicial { get; }
        public int PremioPorInicio { get; }
        public int MaxTurnos { get; }          //cantidad máxima de turnos individuales (jugadas)
        public int MinJugadores { get; }       //para poder probar con menos de 4 tmb
        public bool UsarRfidParaPagos { get; set; }     //el servidor lo activa cuando hay hardware
        public string CarpetaExportacion { get; set; } = Directory.GetCurrentDirectory();

        //Estados
        private readonly object candado = new object();
        private readonly Jugador?[] jugadores = new Jugador?[MaxJugadores];
        private int cantidadJugadores;
        private readonly ColaCircular<Jugador> turnos = new ColaCircular<Jugador>();
        private int[] ordenTurnos = new int[0];
        private readonly Tablero tablero;
        private readonly ListaCircularDoble<CartaEvento> mazo;
        private readonly Banco banco = new Banco();
        private readonly Dado dado = new Dado();
        private readonly int posicionCarcel;

        public EstadoJuego Estado { get; private set; } = EstadoJuego.Esperando;
        public int NumeroTurno { get; private set; }

        private bool dadosLanzados;
        private Propiedad? compraPendiente;
        private PagoPendiente? pagoPendiente;
        private int? jugadorEsperandoVinculacion;
        private Jugador? ganador;
        private string motivoFin = "";
        private string? rutaUltimaExportacion;
        private int profundidadMovimiento;      //evita cadenas infinitas carta -> mover -> carta...
        private ListaDoblementeEnlazada<EventoJuego> eventos = new ListaDoblementeEnlazada<EventoJuego>();

        // Pago obligatorio o compra que espera a que el jugador acerque su tarjeta RFID
        private class PagoPendiente
        {
            public Jugador Deudor = null!;
            public Jugador? Acreedor;          //null = Banco
            public int Monto;
            public TipoTransaccion Tipo;
            public string Descripcion = "";
            public Propiedad? Propiedad;       //distinto de null si es una compra
        }

public Juego(int saldoInicial = 1500, int premioPorInicio = 200, int maxTurnos = 60, int minJugadores = 2, // Constructor: configuración de la partida (valores por defecto)
                     Tablero? tablero = null, ListaCircularDoble<CartaEvento>? mazo = null) // si no pasan tablero o mazo, se usan los de ConfiguracionPartida
        {
            SaldoInicial = saldoInicial;
            PremioPorInicio = premioPorInicio;
            MaxTurnos = maxTurnos;
            MinJugadores = Math.Clamp(minJugadores, 1, MaxJugadores); // entre 1 y 4 jugadores
            this.tablero = tablero ?? ConfiguracionPartida.CrearTableroPorDefecto(); // tablero por defecto (24 casillas)
            this.mazo = mazo ?? ConfiguracionPartida.CrearMazoPorDefecto(); // mazo por defecto (ya barajado)

            if (this.tablero.CantidadCasillas < 24) // el enunciado exige mínimo 24 casillas
            {
                throw new ArgumentException("El tablero debe tener al menos 24 casillas.");
            }

            posicionCarcel = 0; // se busca dónde está la cárcel para mandar jugadores ahí
            foreach (Casilla casilla in this.tablero.Recorrer()) // recorre la lista circular una vuelta
            {
                if (casilla is CasillaEspecial especial && especial.TipoEspecial == TipoCasillaEspecial.Carcel)
                {
                    posicionCarcel = casilla.Posicion;
                    break;
                }
            }
        }

        public int CantidadJugadores => cantidadJugadores; // cuántos jugadores se han registrado

        public Jugador? ObtenerJugador(int jugadorId) // devuelve el jugador por id (1 a 4) o null
        {
            if (jugadorId < 1 || jugadorId > MaxJugadores) return null;
            return jugadores[jugadorId - 1]; // el id 1 está en la posición 0 del arreglo
        }

        public bool ExisteJugador(int jugadorId) => ObtenerJugador(jugadorId) != null; // true si el id es de un jugador registrado

        // Acciones públicas (las llama el servidor)

        public ResultadoAccion AgregarJugador(string nombre) // acción CONECTAR: registra un jugador nuevo
        {
            lock (candado) // un solo hilo modifica el estado a la vez
            {
                IniciarAccion(); // limpia la lista de eventos de esta acción

                if (Estado != EstadoJuego.Esperando) // solo se entra antes de iniciar
                    return Error(CodigosError.AccionInvalida, "La partida ya comenzó.");
                if (cantidadJugadores >= MaxJugadores)
                    return Error(CodigosError.AccionInvalida, $"La partida ya tiene {MaxJugadores} jugadores.");
                if (string.IsNullOrWhiteSpace(nombre))
                    return Error(CodigosError.AccionInvalida, "El nombre del jugador no es válido.");

                int id = cantidadJugadores + 1; // ids consecutivos: 1, 2, 3, 4
                Jugador jugador = new Jugador(id, nombre.Trim());
                jugadores[id - 1] = jugador;
                cantidadJugadores++;

                Emitir(Acciones.JugadorConectado, id, $"{jugador.nombre} se unió a la partida.", // aviso para todos los clientes
                    new { JugadorId = id, Nombre = jugador.nombre, CantidadJugadores = cantidadJugadores });

                ResultadoAccion r = Ok($"Jugador {jugador.nombre} registrado correctamente. ID: {id}");
                r.JugadorId = id; // el server usa este id en la respuesta de CONECTAR
                return r;
            }
        }

        // Solo el organizador (jugador 1) puede iniciar. Se rifa el orden y se reparte el saldo inicial.
        public ResultadoAccion IniciarJuego(int jugadorId) // acción INICIAR_JUEGO
        {
            lock (candado)
            {
                IniciarAccion();

                if (Estado != EstadoJuego.Esperando)
                    return Error(CodigosError.AccionInvalida, "La partida ya fue iniciada.");
                if (jugadorId != 1)
                    return Error(CodigosError.AccionInvalida, "Solo el organizador (jugador 1) puede iniciar la partida.");
                if (cantidadJugadores < MinJugadores)
                    return Error(CodigosError.AccionInvalida, $"Se necesitan al menos {MinJugadores} jugadores.");

                // Rifa del orden (Fisher-Yates) y creación de la cola circular de turnos
                Jugador[] orden = new Jugador[cantidadJugadores]; // copia de los jugadores para barajarla
                for (int i = 0; i < cantidadJugadores; i++) orden[i] = jugadores[i]!;
                for (int i = orden.Length - 1; i > 0; i--) // Fisher-Yates: mezcla aleatoria
                {
                    int j = Random.Shared.Next(i + 1);
                    (orden[i], orden[j]) = (orden[j], orden[i]); // intercambia las dos posiciones
                }

                ordenTurnos = new int[orden.Length]; // ids en orden de turno (para la GUI)
                for (int i = 0; i < orden.Length; i++)
                {
                    Jugador jugador = orden[i];
                    jugador.saldo = SaldoInicial; // todos arrancan con el mismo saldo
                    jugador.posicionActual = 0; // 0 = Inicio
                    jugador.activo = true;
                    jugador.turnosEnCarcel = 0;
                    turnos.Encolar(jugador); // se agrega a la cola circular de turnos
                    ordenTurnos[i] = jugador.id;
                }

                Estado = EstadoJuego.EnCurso; // ya se puede jugar
                NumeroTurno = 1;
                dadosLanzados = false;

                Emitir(Acciones.JuegoIniciado, null, "¡La partida comenzó!", // avisa a todos que empezó
                    new { OrdenTurnos = ordenTurnos, SaldoInicial, MaxTurnos });
                EmitirCambioDeTurno(); // avisa quién juega primero

                return Ok("Partida iniciada.", new { OrdenTurnos = ordenTurnos });
            }
        }

        public ResultadoAccion TirarDados(int jugadorId) // acción TIRAR_DADOS: lanza, mueve y resuelve la casilla
        {
            lock (candado)
            {
                IniciarAccion();

                ResultadoAccion? error = ValidarTurno(jugadorId, out Jugador jugador); // valida estado, jugador y que sea su turno
                if (error != null) return error;
                if (dadosLanzados) // no se puede tirar dos veces en el mismo turno
                    return Error(CodigosError.DadosYaLanzados, "Los dados ya fueron lanzados en este turno.");

                (int valor1, int valor2) = dado.Lanzar(); // dos valores de 1 a 6
                dadosLanzados = true;
                int total = valor1 + valor2;

                Emitir(Acciones.TirarDados, jugador.id, $"{jugador.nombre} lanzó {valor1} y {valor2}.", // todos ven el resultado
                    new DatosDados { Valor1 = valor1, Valor2 = valor2 });

                profundidadMovimiento = 0; // reinicia el contador que evita cadenas infinitas de cartas
                MoverJugador(jugador, total); // mueve nodo por nodo y aplica la casilla (AlCaer)

                Casilla casilla = tablero.ObtenerCasilla(jugador.posicionActual); // casilla donde quedó
                ResultadoAccion r = Ok($"Dados lanzados: {valor1} y {valor2}.", new
                {
                    Valor1 = valor1,
                    Valor2 = valor2,
                    Total = total,
                    Posicion = jugador.posicionActual,
                    Casilla = casilla.Nombre,
                    CompraDisponible = compraPendiente != null, // la GUI muestra el botón Comprar si es true
                    PagoPendiente = pagoPendiente != null // la GUI pide la tarjeta si es true
                });
                r.JugadorId = jugador.id;
                r.Dados = new DatosDados { Valor1 = valor1, Valor2 = valor2 }; // el server manda estos valores al display del hardware
                return r;
            }
        }

        // Botón físico del hardware: tira por el jugador que tiene el turno.
        public ResultadoAccion TirarDadosJugadorActual() // BOTON_PRESIONADO
        {
            lock (candado)
            {
                if (Estado != EstadoJuego.EnCurso)
                {
                    IniciarAccion();
                    return Error(CodigosError.JuegoNoIniciado, "La partida no está en curso.");
                }
                return TirarDados(turnos.Actual().id); // tira por quien tenga el turno
            }
        }

        public ResultadoAccion ComprarPropiedad(int jugadorId) // acción COMPRAR_PROPIEDAD
        {
            lock (candado)
            {
                IniciarAccion();

                ResultadoAccion? error = ValidarTurno(jugadorId, out Jugador jugador);
                if (error != null) return error;
                if (pagoPendiente != null) // primero se resuelve el pago que espera tarjeta
                    return Error(CodigosError.PagoPendiente, "Hay un pago pendiente: acerque su tarjeta RFID.");
                if (!dadosLanzados) // solo se compra después de tirar
                    return Error(CodigosError.AccionInvalida, "Debe lanzar los dados antes de comprar.");

                Casilla casilla = tablero.ObtenerCasilla(jugador.posicionActual);
                if (casilla is not Propiedad propiedad) // debe estar parado en una propiedad
                    return Error(CodigosError.AccionInvalida, "La casilla actual no es una propiedad.");
                if (propiedad.Propietario != null) // no se compra lo que ya tiene dueño
                    return Error(CodigosError.PropiedadYaVendida, $"{propiedad.Nombre} ya tiene propietario.");
                if (!ReferenceEquals(compraPendiente, propiedad)) // solo la propiedad donde cayó en este turno
                    return Error(CodigosError.AccionInvalida, "No hay una compra disponible en este turno.");
                if (!banco.TieneSaldoSuficiente(jugador, propiedad.Precio)) // se valida el saldo antes de cobrar
                    return Error(CodigosError.SaldoInsuficiente,
                        $"Saldo insuficiente: {propiedad.Nombre} cuesta {propiedad.Precio} y tiene {jugador.saldo}.");

                compraPendiente = null; // la oferta ya se usó

                if (RequiereRfid(jugador)) // con tarjeta vinculada, la compra espera el RFID
                {
                    pagoPendiente = new PagoPendiente
                    {
                        Deudor = jugador, Acreedor = null, Monto = propiedad.Precio,
                        Tipo = TipoTransaccion.CompraPropiedad, Descripcion = $"Compra de {propiedad.Nombre}",
                        Propiedad = propiedad
                    };
                    EmitirPagoPendiente(); // avisa a todos; el server manda ESPERAR_RFID al hardware
                    return Ok($"Acerque su tarjeta RFID para confirmar la compra de {propiedad.Nombre}.");
                }

                EjecutarCompra(jugador, propiedad); // sin RFID: se cobra de una vez
                return Ok($"Compró {propiedad.Nombre} por {propiedad.Precio}.");
            }
        }

        public ResultadoAccion NoComprar(int jugadorId) // acción NO_COMPRAR
        {
            lock (candado)
            {
                IniciarAccion();

                ResultadoAccion? error = ValidarTurno(jugadorId, out Jugador jugador);
                if (error != null) return error;

                // Permite arrepentirse de una compra que estaba esperando la tarjeta
                if (pagoPendiente != null && pagoPendiente.Propiedad != null)
                {
                    string nombre = pagoPendiente.Propiedad.Nombre;
                    pagoPendiente = null;
                    Emitir(Acciones.MensajeJuego, jugador.id, $"{jugador.nombre} canceló la compra de {nombre}.", null);
                    return Ok("Compra cancelada.");
                }
                if (compraPendiente == null) // no hay nada que rechazar
                    return Error(CodigosError.AccionInvalida, "No hay una compra disponible.");

                RechazarCompraPendiente(jugador); // descarta la oferta y avisa
                return Ok("Decidió no comprar.");
            }
        }

        public ResultadoAccion TerminarTurno(int jugadorId) // acción TERMINAR_TURNO
        {
            lock (candado)
            {
                IniciarAccion();

                ResultadoAccion? error = ValidarTurno(jugadorId, out Jugador jugador);
                if (error != null) return error;
                if (!dadosLanzados) // hay que tirar antes de terminar
                    return Error(CodigosError.AccionInvalida, "Debe lanzar los dados antes de terminar el turno.");
                if (pagoPendiente != null) // no se puede pasar el turno debiendo
                    return Error(CodigosError.PagoPendiente, "Tiene un pago pendiente: acerque su tarjeta RFID.");

                // Terminar turno con una compra sin decidir cuenta como NO_COMPRAR
                if (compraPendiente != null) RechazarCompraPendiente(jugador);

                IniciarSiguienteTurno(avanzarCola: true); // pasa al siguiente jugador de la cola

                if (Estado == EstadoJuego.Finalizado) // el turno pudo cerrar la partida (límite de turnos)
                    return Ok("Turno terminado. La partida finalizó.");

                Jugador siguiente = turnos.Actual(); // jugador que sigue
                return Ok($"Turno terminado. Ahora juega {siguiente.nombre}.", new
                {
                    NumeroTurno,
                    SiguienteJugadorId = siguiente.id,
                    SiguienteJugador = siguiente.nombre
                });
            }
        }

        // Desconexión durante una partida: se considera abandono y elimina al jugador.
        // No se utiliza EliminarJugador porque ese método representa una quiebra y puede
        // realizar movimientos de dinero. En un abandono solo se retira al jugador.
        public ResultadoAccion AbandonarJugador(int jugadorId)
        {
            lock (candado)
            {
                IniciarAccion();

                if (Estado != EstadoJuego.EnCurso) //Si no hay partida en curso no lo elimina
                    return Error(CodigosError.AccionInvalida,"No hay una partida en curso.");

                Jugador? jugador = ObtenerJugador(jugadorId);

                if (jugador == null) //Si no existe el jugador
                    return Error(CodigosError.JugadorNoEncontrado,"El jugador indicado no existe.");

                if (!jugador.activo) //Si no esta activo
                    return Error(CodigosError.JugadorEliminado,"El jugador ya estaba eliminado.");

                // Se guarda antes de quitarlo de la cola para saber si debemos
                // comenzar inmediatamente el turno del siguiente jugador.
                bool eraSuTurno = ReferenceEquals(turnos.Actual(), jugador);

                // El jugador deja oficialmente la partida.
                jugador.LiberarPropiedades();
                jugador.activo = false;
                jugador.turnosEnCarcel = 0;

                // Si estaba esperando vincular una tarjeta RFID, se cancela.
                if (jugadorEsperandoVinculacion == jugador.id)
                {
                    jugadorEsperandoVinculacion = null;
                }

                // Si abandonó durante su turno, cualquier acción pendiente de ese
                // turno se descarta para evitar que la partida quede bloqueada.
                if (eraSuTurno)
                {
                    compraPendiente = null;
                    pagoPendiente = null;
                    dadosLanzados = false;
                }

                Emitir(
                    Acciones.JugadorEliminado,
                    jugador.id,
                    $"{jugador.nombre} se desconectó y quedó fuera de la partida.",
                    new
                    {
                        JugadorId = jugador.id,
                        Nombre = jugador.nombre,
                        Motivo = "Desconexion"
                    }
                );

                // Lo elimina de la cola circular de turnos.
                QuitarDeLaCola(jugador);

                // Si queda un único jugador activo, gana automáticamente.
                if (turnos.Size <= 1)
                {
                    Finalizar(turnos.EstaVacia() ? null : turnos.Actual(),"Único jugador activo");

                    return Ok($"{jugador.nombre} abandonó la partida. La partida finalizó.");
                }

                // Si era el jugador actual, EliminarActual ya dejó la cola apuntando
                // al siguiente. No se debe avanzar otra vez.
                if (eraSuTurno)
                {
                    IniciarSiguienteTurno(avanzarCola: false);
                }

                return Ok($"{jugador.nombre} abandonó la partida.");
            }
        }

        // RFID

        // Paso 1 de la vinculación: el servidor luego manda ESPERAR_RFID al hardware.
        public ResultadoAccion SolicitarVinculacionRfid(int jugadorId) // acción VINCULAR_RFID
        {
            lock (candado)
            {
                IniciarAccion();

                if (!ExisteJugador(jugadorId))
                    return Error(CodigosError.JugadorNoEncontrado, "El jugador indicado no existe.");
                if (jugadorEsperandoVinculacion.HasValue && jugadorEsperandoVinculacion != jugadorId) // solo un jugador vinculando a la vez
                    return Error(CodigosError.AccionInvalida, "Ya hay otro jugador esperando vincular una tarjeta.");

                jugadorEsperandoVinculacion = jugadorId; // la próxima tarjeta que llegue se le asigna
                ResultadoAccion r = Ok("Acerque una tarjeta al lector RFID.");
                r.NotificarEstado = false; // no cambia nada visible del juego
                r.JugadorId = jugadorId;
                return r;
            }
        }

        public void CancelarVinculacionRfid() // se usa si no se pudo avisar al hardware
        {
            lock (candado) { jugadorEsperandoVinculacion = null; }
        }

        // Punto de entrada de TODA lectura de tarjeta. Decide si es vinculación o pago.
        public ResultadoAccion ProcesarRfid(string uid) // acción RFID_DETECTADO
        {
            lock (candado)
            {
                IniciarAccion();

                uid = (uid ?? "").Trim().ToUpperInvariant(); // normaliza el UID (sin espacios, en mayúscula)
                if (uid.Length == 0)
                    return Error(CodigosError.AccionInvalida, "El UID recibido no es válido.");

                // 1) Vinculación pendiente
                if (jugadorEsperandoVinculacion.HasValue)
                {
                    Jugador jugador = ObtenerJugador(jugadorEsperandoVinculacion.Value)!; // jugador que pidió vincular
                    Jugador? dueno = BuscarPorTarjeta(uid); // ¿la tarjeta ya es de alguien?
                    if (dueno != null && !ReferenceEquals(dueno, jugador))
                        return Error(CodigosError.AccionInvalida, "Esta tarjeta RFID ya está vinculada a otro jugador.");

                    jugador.tarjetaRfid = uid; // queda vinculada
                    jugadorEsperandoVinculacion = null;
                    Emitir(Acciones.RfidVinculado, jugador.id, $"Tarjeta vinculada a {jugador.nombre}.",
                        new DatosRfid { UID = uid });

                    ResultadoAccion r = Ok($"RFID vinculado correctamente al jugador {jugador.nombre}.");
                    r.JugadorId = jugador.id;
                    return r;
                }

                // 2) Pago pendiente de confirmación
                if (pagoPendiente != null)
                {
                    Jugador? jugador = BuscarPorTarjeta(uid); // dueño de la tarjeta
                    if (jugador == null)
                        return Error(CodigosError.JugadorNoEncontrado, "La tarjeta no está vinculada a ningún jugador.");
                    if (!ReferenceEquals(jugador, pagoPendiente.Deudor)) // solo sirve la tarjeta de quien debe pagar
                        return Error(CodigosError.FueraDeTurno,
                            $"Esa tarjeta es de {jugador.nombre}; debe pagar {pagoPendiente.Deudor.nombre}.");

                    EjecutarPagoPendiente(); // cobra la compra o el pago que esperaba
                    ResultadoAccion r = Ok("Pago confirmado con tarjeta RFID.");
                    r.JugadorId = jugador.id;
                    return r;
                }

                // 3) Nada que hacer con la tarjeta
                ResultadoAccion nada = Ok("Tarjeta detectada, no hay operación pendiente."); // la lectura se ignora
                nada.NotificarEstado = false;
                nada.JugadorId = BuscarPorTarjeta(uid)?.id;
                return nada;
            }
        }

        // Si se cae el hardware, los pagos pasan a ser automáticos y se resuelve lo pendiente.
        public ResultadoAccion DesactivarRfid() // el server lo llama si se desconecta la Raspberry
        {
            lock (candado)
            {
                IniciarAccion();
                UsarRfidParaPagos = false; // desde ahora los pagos son automáticos
                jugadorEsperandoVinculacion = null;
                if (pagoPendiente != null)
                {
                    Emitir(Acciones.MensajeJuego, pagoPendiente.Deudor.id,
                        "Se perdió el lector RFID: el pago pendiente se aplica automáticamente.", null);
                    EjecutarPagoPendiente(); // cobra lo que esperaba tarjeta
                }
                return Ok("Pagos con RFID desactivados.");
            }
        }

        // Consultas

        public EstadoJuegoDto ObtenerEstado() // CONSULTAR_ESTADO / ESTADO_ACTUALIZADO: foto completa para la GUI
        {
            lock (candado)
            {
                EstadoJuegoDto estado = new EstadoJuegoDto
                {
                    Estado = Estado.ToString(),
                    NumeroTurno = NumeroTurno,
                    MaxTurnos = MaxTurnos,
                    JugadorEnTurnoId = Estado == EstadoJuego.EnCurso && !turnos.EstaVacia() ? turnos.Actual().id : null, // null si la partida no está en curso
                    DadosLanzados = dadosLanzados,
                    Dado1 = dado.Valor1,
                    Dado2 = dado.Valor2,
                    CompraPendientePosicion = compraPendiente?.Posicion, // null si no hay compra disponible
                    PagoPendiente = CrearPagoPendienteDto(),
                    RfidParaPagos = UsarRfidParaPagos,
                    GanadorId = ganador?.id,
                    OrdenTurnos = ordenTurnos,
                    Jugadores = new JugadorDto[cantidadJugadores], // se llena abajo
                    Casillas = new CasillaDto[tablero.CantidadCasillas] // se llena abajo
                };

                for (int i = 0; i < cantidadJugadores; i++) // un DTO por jugador
                {
                    estado.Jugadores[i] = CrearJugadorDto(jugadores[i]!);
                }

                int k = 0; // índice del arreglo de casillas
                foreach (Casilla casilla in tablero.Recorrer()) // recorre la lista circular del tablero
                {
                    CasillaDto dto = new CasillaDto
                    {
                        Posicion = casilla.Posicion,
                        Nombre = casilla.Nombre,
                        Tipo = casilla.Tipo.ToString()
                    };
                    if (casilla is Propiedad propiedad) // datos solo de propiedades
                    {
                        dto.Precio = propiedad.Precio;
                        dto.Alquiler = (int)propiedad.CalcularAlquiler();
                        dto.PropietarioId = propiedad.Propietario?.id; // null si no tiene dueño
                    }
                    else if (casilla is CasillaEspecial especial) // datos solo de especiales (impuesto, cárcel...)
                    {
                        dto.Subtipo = especial.TipoEspecial.ToString();
                        dto.Monto = especial.Monto;
                    }
                    estado.Casillas[k++] = dto;
                }
                return estado;
            }
        }

        // jugadorId y tipo son filtros opcionales; desdeInicio = de la más antigua a la más reciente.
        public TransaccionDto[] ConsultarTransacciones(int? jugadorId = null, TipoTransaccion? tipo = null, bool desdeInicio = true) // acción CONSULTAR_TRANSACCIONES
        {
            lock (candado)
            {
                IEnumerable<Transaccion> origen;
                if (jugadorId.HasValue) origen = banco.ConsultarHistorialPorJugador(jugadorId.Value); // búsqueda por jugador
                else if (tipo.HasValue) origen = banco.ConsultarHistorialPorTipo(tipo.Value); // búsqueda por tipo
                else origen = banco.ConsultarHistorialCompleto(desdeInicio); // todo el historial en el orden pedido

                // Si piden ambos filtros o el orden inverso con filtro, se aplica aquí
                int cantidad = 0;
                foreach (Transaccion t in origen) // primera pasada: contar para crear el arreglo
                    if (!tipo.HasValue || t.Tipo == tipo.Value) cantidad++;

                TransaccionDto[] resultado = new TransaccionDto[cantidad];
                int i = 0;
                foreach (Transaccion t in origen) // segunda pasada: llenar el arreglo
                    if (!tipo.HasValue || t.Tipo == tipo.Value) resultado[i++] = CrearTransaccionDto(t);

                if (!desdeInicio && (jugadorId.HasValue || tipo.HasValue)) // las búsquedas filtradas vienen de la más antigua: se invierten
                    Array.Reverse(resultado);

                return resultado;
            }
        }

        // Exporta el historial completo a TXT. Devuelve la ruta del archivo generado.
        public string ExportarTransacciones(string? ruta = null) // acción EXPORTAR_TRANSACCIONES (también se llama al terminar)
        {
            lock (candado)
            {
                ruta ??= Path.Combine(CarpetaExportacion, $"transacciones_{DateTime.Now:yyyyMMdd_HHmmss}.txt"); // nombre por defecto con fecha y hora

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("MONOPOLY DISTRIBUIDO - HISTORIAL DE TRANSACCIONES");
                sb.AppendLine($"Generado: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"Estado de la partida: {Estado}   Turno actual: {NumeroTurno}   Transacciones: {banco.CantidadTransacciones}");
                sb.AppendLine(new string('=', 130)); // línea separadora dentro del TXT
                sb.AppendLine($"{"N°",-5}{"Turno",-7}{"Fecha y hora",-21}{"Tipo",-22}{"Origen",-16}{"Destino",-16}{"Monto",8}  Descripción"); // encabezados de las columnas
                sb.AppendLine(new string('-', 130));

                foreach (Transaccion t in banco.ConsultarHistorialCompleto(true)) // de la más antigua a la más reciente
                {
                    sb.AppendLine($"{t.Id,-5}{t.NumeroTurno,-7}{t.FechaHora:yyyy-MM-dd HH:mm:ss}  {t.Tipo,-22}" +
                                  $"{Recortar(NombreDe(t.JugadorOrigenId), 15),-16}{Recortar(NombreDe(t.JugadorDestinoId), 15),-16}" +
                                  $"{t.Monto,8}  {t.Descripcion}");
                }

                sb.AppendLine(new string('=', 130));
                sb.AppendLine("RESUMEN DE JUGADORES"); // al final, resumen de cada jugador
                for (int i = 0; i < cantidadJugadores; i++)
                {
                    Jugador j = jugadores[i]!;
                    sb.AppendLine($"  {j.id}. {j.nombre,-15} Saldo: {j.saldo,6}  Propiedades: {j.CantidadPropiedades,2}  " +
                                  $"Patrimonio: {j.PatrimonioTotal(),6}  {(j.activo ? "Activo" : "Eliminado")}");
                }
                if (Estado == EstadoJuego.Finalizado)
                {
                    sb.AppendLine($"GANADOR: {ganador?.nombre ?? "-"} ({motivoFin})");
                }

                File.WriteAllText(ruta, sb.ToString(), Encoding.UTF8); // escribe el archivo
                rutaUltimaExportacion = Path.GetFullPath(ruta); // guarda la ruta completa
                return rutaUltimaExportacion;
            }
        }

        // Métodos que usan las casillas y cartas (polimorfismo)

        internal void Informar(Jugador jugador, string mensaje) // mensaje informativo para todos
        {
            Emitir(Acciones.MensajeJuego, jugador.id, mensaje, null);
        }

        internal void OfrecerCompra(Jugador jugador, Propiedad propiedad) // propiedad libre: guarda la oferta de compra
        {
            compraPendiente = propiedad; // el jugador decide con COMPRAR o NO_COMPRAR
            bool alcanza = jugador.saldo >= propiedad.Precio; // solo informativo; la validación real está en ComprarPropiedad
            Emitir(Acciones.CompraDisponible, jugador.id,
                alcanza ? $"{jugador.nombre} puede comprar {propiedad.Nombre} por {propiedad.Precio}."
                        : $"{jugador.nombre} cayó en {propiedad.Nombre} pero no le alcanza ({propiedad.Precio}).",
                new
                {
                    Posicion = propiedad.Posicion,
                    Nombre = propiedad.Nombre,
                    Precio = propiedad.Precio,
                    Alquiler = (int)propiedad.CalcularAlquiler(),
                    SaldoSuficiente = alcanza
                });
        }

        // Pago que el jugador NO puede evitar (alquiler, impuesto, carta).
        // Si no le alcanza queda eliminado; si usa RFID queda pendiente hasta que pase la tarjeta.
        internal void CobrarObligatorio(Jugador deudor, Jugador? acreedor, int monto, TipoTransaccion tipo, string descripcion)
        {
            if (monto <= 0) return; // nada que cobrar

            if (deudor.saldo < monto) // no le alcanza: queda eliminado
            {
                EliminarJugador(deudor, acreedor, tipo, descripcion, monto);
                return;
            }

            if (RequiereRfid(deudor)) // con tarjeta: espera el RFID
            {
                pagoPendiente = new PagoPendiente
                {
                    Deudor = deudor, Acreedor = acreedor, Monto = monto, Tipo = tipo, Descripcion = descripcion
                };
                EmitirPagoPendiente();
                return;
            }

            EjecutarPago(deudor, acreedor, monto, tipo, descripcion); // sin tarjeta: se cobra de una vez
        }

        internal void PagarDesdeBanco(Jugador jugador, int monto, TipoTransaccion tipo, string descripcion) // ganancias de cartas y premio por Inicio
        {
            if (monto <= 0) return;
            EjecutarPago(null, jugador, monto, tipo, descripcion); // origen null = paga el Banco
        }

        internal CartaEvento TomarCarta(Jugador jugador) // CasillaEvento la llama al caer
        {
            CartaEvento carta = mazo.TomarPrimeroYEnviarAlFinal(); //la carta usada pasa al final
            Emitir(Acciones.CartaTomada, jugador.id, $"{jugador.nombre} tomó una carta: {carta.Descripcion}", new
            {
                CartaId = carta.Id,
                Descripcion = carta.Descripcion,
                Efecto = carta.TipoEfecto.ToString(),
                Valor = (int)carta.Valor
            });
            return carta;
        }

        // Avanza (pasos > 0) o retrocede (pasos < 0) nodo por nodo y resuelve la casilla de llegada.
        internal void MoverJugador(Jugador jugador, int pasos)
        {
            if (pasos == 0 || !jugador.activo) return; // los eliminados no se mueven

            int desde = jugador.posicionActual;
            int[] recorrido = tablero.RecorrerDesde(desde, pasos); // posiciones visitadas nodo por nodo
            int hasta = recorrido[recorrido.Length - 1]; // la última es donde queda
            jugador.posicionActual = hasta;

            Casilla destino = tablero.ObtenerCasilla(hasta);
            Emitir(Acciones.JugadorMovido, jugador.id, $"{jugador.nombre} se movió a {destino.Nombre}.", new
            {
                JugadorId = jugador.id,
                Desde = desde,
                Hasta = hasta,
                Recorrido = recorrido,
                Casilla = destino.Nombre
            });

            // Solo se cobra el premio avanzando (no retrocediendo) y si el recorrido tocó Inicio
            if (pasos > 0 && Array.IndexOf(recorrido, 0) >= 0) // busca el 0 (Inicio) dentro del recorrido
            {
                PagarDesdeBanco(jugador, PremioPorInicio, TipoTransaccion.PremioPorPasarInicio, "Premio por pasar por Inicio");
            }

            ResolverCasillaActual(jugador); // aplica la casilla de llegada
        }

        // Avanza hasta una casilla específica (siempre hacia adelante, cobra Inicio si lo pasa)
        internal void MoverJugadorA(Jugador jugador, int posicionDestino)
        {
            int total = tablero.CantidadCasillas;
            int pasos = ((posicionDestino - jugador.posicionActual) % total + total) % total; // pasos hacia adelante hasta el destino
            if (pasos == 0) pasos = total; //ya estaba ahí: da la vuelta completa
            MoverJugador(jugador, pasos);
        }

        // Directo a la cárcel: no pasa por Inicio, no cobra premio, pierde 1 turno.
        internal void EnviarACarcel(Jugador jugador)
        {
            int desde = jugador.posicionActual;
            jugador.posicionActual = posicionCarcel; // salta directo, sin recorrer
            jugador.turnosEnCarcel = 1; // pierde 1 turno
            compraPendiente = null; // no puede comprar desde la cárcel
            Emitir(Acciones.JugadorMovido, jugador.id, $"{jugador.nombre} fue enviado a la cárcel y pierde un turno.", new
            {
                JugadorId = jugador.id,
                Desde = desde,
                Hasta = posicionCarcel,
                Recorrido = new[] { posicionCarcel },
                Casilla = tablero.ObtenerCasilla(posicionCarcel).Nombre
            });
        }

        internal void HacerPerderTurnos(Jugador jugador, int cantidad)
        {
            jugador.turnosEnCarcel += cantidad; // se descuentan en IniciarSiguienteTurno
            Informar(jugador, $"{jugador.nombre} perderá {cantidad} turno(s).");
        }

        // Lógica interna

        private void ResolverCasillaActual(Jugador jugador)
        {
            if (profundidadMovimiento >= 3 || !jugador.activo) return; // máximo 3 casillas encadenadas por tirada
            profundidadMovimiento++;
            try
            {
                tablero.ObtenerCasilla(jugador.posicionActual).AlCaer(jugador, this); //polimorfismo
            }
            finally
            {
                profundidadMovimiento--;
            }
        }

        private void EjecutarPago(Jugador? origen, Jugador? destino, int monto, TipoTransaccion tipo, string descripcion)
        {
            Transaccion t = banco.Pagar(origen, destino, monto, tipo, descripcion, NumeroTurno); // Banco cambia saldos y registra la transacción
            Emitir(Acciones.PagoRealizado, origen?.id ?? destino?.id, $"{descripcion}: {monto}", CrearTransaccionDto(t)); // avisa a todos con la transacción
        }

        private void EjecutarCompra(Jugador jugador, Propiedad propiedad)
        {
            EjecutarPago(jugador, null, propiedad.Precio, TipoTransaccion.CompraPropiedad, $"Compra de {propiedad.Nombre}"); // el jugador paga al Banco
            propiedad.AsignarPropietario(jugador); // la propiedad queda a su nombre
            jugador.AgregarPropiedad(propiedad); // se agrega a su lista de propiedades
            Emitir(Acciones.PropiedadComprada, jugador.id, $"{jugador.nombre} compró {propiedad.Nombre}.", new
            {
                JugadorId = jugador.id,
                Posicion = propiedad.Posicion,
                Nombre = propiedad.Nombre,
                Precio = propiedad.Precio
            });
        }

        private void EjecutarPagoPendiente()
        {
            PagoPendiente? p = pagoPendiente;
            pagoPendiente = null; // se limpia antes de cobrar
            if (p == null) return;

            if (p.Propiedad != null) // era una compra
            {
                if (p.Propiedad.Propietario == null && p.Deudor.saldo >= p.Propiedad.Precio) // se revalida por si algo cambió
                    EjecutarCompra(p.Deudor, p.Propiedad);
                else
                    Informar(p.Deudor, $"No se pudo completar la compra de {p.Propiedad.Nombre}.");
                return;
            }

            if (p.Deudor.saldo < p.Monto) // era un pago obligatorio
                EliminarJugador(p.Deudor, p.Acreedor, p.Tipo, p.Descripcion, p.Monto);
            else
                EjecutarPago(p.Deudor, p.Acreedor, p.Monto, p.Tipo, p.Descripcion);
        }

        private void RechazarCompraPendiente(Jugador jugador)
        {
            string nombre = compraPendiente!.Nombre; // ! = ya sabemos que no es null
            compraPendiente = null;
            Emitir(Acciones.MensajeJuego, jugador.id, $"{jugador.nombre} decidió no comprar {nombre}.", null);
        }

        // No pudo cubrir un pago obligatorio: entrega lo que tiene, libera propiedades y sale de la cola.
        private void EliminarJugador(Jugador jugador, Jugador? acreedor, TipoTransaccion tipo, string descripcion, int montoAdeudado)
        {
            if (jugador.saldo > 0) // entrega lo que le queda
            {
                EjecutarPago(jugador, acreedor, jugador.saldo, tipo, $"{descripcion} (pago parcial, debía {montoAdeudado})");
            }

            jugador.LiberarPropiedades(); // sus propiedades vuelven a estar libres
            jugador.activo = false;
            jugador.turnosEnCarcel = 0;
            compraPendiente = null;
            pagoPendiente = null;

            Emitir(Acciones.JugadorEliminado, jugador.id,
                $"{jugador.nombre} quedó eliminado: no pudo pagar {montoAdeudado} ({descripcion}).",
                new { JugadorId = jugador.id, Nombre = jugador.nombre, MontoAdeudado = montoAdeudado });

            bool eraSuTurno = QuitarDeLaCola(jugador); // sale de la cola circular

            if (turnos.Size <= 1) // queda uno solo: gana
            {
                Finalizar(turnos.EstaVacia() ? null : turnos.Actual(), "Único jugador activo");
                return;
            }

            // EliminarActual ya dejó el turno en el siguiente jugador: no se avanza la cola otra vez
            if (eraSuTurno) IniciarSiguienteTurno(avanzarCola: false); // turno del siguiente
        }

        // Quita a un jugador de la cola circular. Devuelve true si era el del turno actual.
        private bool QuitarDeLaCola(Jugador jugador)
        {
            Jugador actual = turnos.Actual();
            if (ReferenceEquals(actual, jugador))
            {
                turnos.EliminarActual(); // el turno pasa solo al siguiente
                return true;
            }

            // Caso raro (no debería pasar: solo paga el jugador en turno). Se rota hasta él, se elimina
            // y se vuelve al jugador que tenía el turno.
            for (int i = 0; i < turnos.Size && !ReferenceEquals(turnos.Actual(), jugador); i++) turnos.AvanzarTurno(); // rota hasta el jugador
            if (ReferenceEquals(turnos.Actual(), jugador)) turnos.EliminarActual();
            for (int i = 0; i < turnos.Size && !ReferenceEquals(turnos.Actual(), actual); i++) turnos.AvanzarTurno(); // vuelve al turno original
            return false;
        }

        // Pasa al siguiente turno saltando a quien deba perder turnos. Verifica el límite de turnos.
        private void IniciarSiguienteTurno(bool avanzarCola)
        {
            dadosLanzados = false;
            compraPendiente = null;
            pagoPendiente = null;

            if (avanzarCola) turnos.AvanzarTurno(); // normal: siguiente de la cola
            NumeroTurno++;

            while (true) // salta a los que deben perder turno
            {
                if (NumeroTurno > MaxTurnos) // se alcanzó el límite de turnos
                {
                    NumeroTurno = MaxTurnos;
                    FinalizarPorLimiteDeTurnos();
                    return;
                }

                Jugador actual = turnos.Actual();
                if (actual.turnosEnCarcel <= 0) break; // puede jugar: sale del ciclo

                actual.turnosEnCarcel--; // se consume un turno perdido
                Emitir(Acciones.TurnoPerdido, actual.id, $"{actual.nombre} pierde este turno.",
                    new { JugadorId = actual.id, TurnosRestantes = actual.turnosEnCarcel, NumeroTurno });

                turnos.AvanzarTurno();
                NumeroTurno++;
            }

            EmitirCambioDeTurno(); // avisa quién juega ahora
        }

        private void FinalizarPorLimiteDeTurnos()
        {
            Jugador? mejor = null;
            for (int i = 0; i < cantidadJugadores; i++)
            {
                Jugador j = jugadores[i]!;
                if (j.activo && (mejor == null || j.PatrimonioTotal() > mejor.PatrimonioTotal())) mejor = j; // mayor patrimonio = saldo + propiedades
            }
            Finalizar(mejor, $"Se alcanzó el límite de {MaxTurnos} turnos (gana el mayor patrimonio)");
        }

        private void Finalizar(Jugador? ganadorPartida, string motivo)
        {
            Estado = EstadoJuego.Finalizado; // ya no se aceptan jugadas
            ganador = ganadorPartida;
            motivoFin = motivo;
            dadosLanzados = false;
            compraPendiente = null;
            pagoPendiente = null;

            // Ranking por patrimonio (arreglo + Array.Sort, no List)
            JugadorDto[] ranking = new JugadorDto[cantidadJugadores];
            for (int i = 0; i < cantidadJugadores; i++) ranking[i] = CrearJugadorDto(jugadores[i]!);
            Array.Sort(ranking, (a, b) => b.Patrimonio.CompareTo(a.Patrimonio)); // de mayor a menor patrimonio

            string? ruta = null;
            try { ruta = ExportarTransacciones(); } catch (Exception) { /* si falla el disco no se cae el juego */ }

            Emitir(Acciones.FinJuego, ganador?.id, // avisa a todos el ganador y el ranking
                $"Fin de la partida. Ganador: {ganador?.nombre ?? "nadie"} ({motivo}).",
                new { GanadorId = ganador?.id, Ganador = ganador?.nombre, Motivo = motivo, Ranking = ranking, ArchivoTransacciones = ruta });
        }

        // Métodos auxiliares

        private void IniciarAccion() // cada acción arranca con su propia lista de eventos
        {
            eventos = new ListaDoblementeEnlazada<EventoJuego>();
        }

        private void Emitir(string accion, int? jugadorId, string mensaje, object? datos) // guarda un aviso para mandarlo a todos los clientes
        {
            eventos.AgregarAlFinal(new EventoJuego(accion, jugadorId, mensaje, datos));
        }

        private ResultadoAccion Ok(string mensaje, object? datos = null) // respuesta exitosa con los eventos acumulados
        {
            ResultadoAccion r = ResultadoAccion.Ok(mensaje, datos);
            r.AsignarEventos(eventos);
            return r;
        }

        private static ResultadoAccion Error(string codigo, string mensaje) // respuesta de error (sin eventos)
        {
            return ResultadoAccion.Error(codigo, mensaje);
        }

        private ResultadoAccion? ValidarTurno(int jugadorId, out Jugador jugador) // validaciones comunes de todas las jugadas
        {
            jugador = null!; // se asigna al final si todo está bien
            if (Estado == EstadoJuego.Esperando)
                return Error(CodigosError.JuegoNoIniciado, "La partida todavía no ha iniciado.");
            if (Estado == EstadoJuego.Finalizado)
                return Error(CodigosError.AccionInvalida, "La partida ya terminó.");

            Jugador? j = ObtenerJugador(jugadorId);
            if (j == null)
                return Error(CodigosError.JugadorNoEncontrado, "El jugador indicado no existe.");
            if (!j.activo)
                return Error(CodigosError.JugadorEliminado, "El jugador está eliminado.");
            if (!ReferenceEquals(turnos.Actual(), j)) // fuera de turno
                return Error(CodigosError.FueraDeTurno, "No es el turno de este jugador.");

            jugador = j;
            return null;
        }

        private bool RequiereRfid(Jugador jugador) // true si el pago se confirma con tarjeta
        {
            return UsarRfidParaPagos && pagoPendiente == null && !string.IsNullOrEmpty(jugador.tarjetaRfid);
        }

        private Jugador? BuscarPorTarjeta(string uid) // busca al dueño de una tarjeta
        {
            for (int i = 0; i < cantidadJugadores; i++)
            {
                Jugador j = jugadores[i]!;
                if (string.Equals(j.tarjetaRfid, uid, StringComparison.OrdinalIgnoreCase)) return j;
            }
            return null;
        }

        private void EmitirCambioDeTurno() // evento TURNO_CAMBIADO
        {
            Jugador actual = turnos.Actual();
            Emitir(Acciones.TurnoCambiado, actual.id, $"Turno {NumeroTurno}: juega {actual.nombre}.",
                new { NumeroTurno, JugadorId = actual.id, Nombre = actual.nombre });
        }

        private void EmitirPagoPendiente() // evento PAGO_PENDIENTE (el server también avisa al hardware)
        {
            PagoPendienteDto dto = CrearPagoPendienteDto()!;
            Emitir(Acciones.PagoPendiente, dto.JugadorId,
                $"{pagoPendiente!.Deudor.nombre} debe pagar {dto.Monto} ({dto.Descripcion}). Acerque su tarjeta RFID.", dto);
        }

        private PagoPendienteDto? CrearPagoPendienteDto() // null si no hay pago pendiente
        {
            if (pagoPendiente == null) return null;
            return new PagoPendienteDto
            {
                JugadorId = pagoPendiente.Deudor.id,
                AcreedorId = pagoPendiente.Acreedor?.id,
                Monto = pagoPendiente.Monto,
                Descripcion = pagoPendiente.Descripcion,
                EsCompra = pagoPendiente.Propiedad != null
            };
        }

        private JugadorDto CrearJugadorDto(Jugador j) // convierte un Jugador al formato de la GUI
        {
            int[] propiedades = new int[j.CantidadPropiedades];
            int i = 0;
            foreach (Propiedad p in j.ObtenerPropiedades()) propiedades[i++] = p.Posicion; // posiciones de sus propiedades

            return new JugadorDto
            {
                Id = j.id,
                Nombre = j.nombre,
                Saldo = j.saldo,
                Posicion = j.posicionActual,
                Activo = j.activo,
                TieneTarjeta = !string.IsNullOrEmpty(j.tarjetaRfid),
                TurnosPorPerder = j.turnosEnCarcel,
                Patrimonio = j.PatrimonioTotal(),
                Propiedades = propiedades
            };
        }

        private TransaccionDto CrearTransaccionDto(Transaccion t) // convierte una Transaccion al formato de la GUI
        {
            return new TransaccionDto
            {
                Id = t.Id,
                FechaHora = t.FechaHora,
                NumeroTurno = t.NumeroTurno,
                Tipo = t.Tipo.ToString(),
                OrigenId = t.JugadorOrigenId,
                Origen = NombreDe(t.JugadorOrigenId),
                DestinoId = t.JugadorDestinoId,
                Destino = NombreDe(t.JugadorDestinoId),
                Monto = t.Monto,
                Descripcion = t.Descripcion ?? ""
            };
        }

        private string NombreDe(int? jugadorId) // nombre para mostrar; null = Banco
        {
            if (!jugadorId.HasValue) return "Banco";
            return ObtenerJugador(jugadorId.Value)?.nombre ?? $"Jugador {jugadorId}";
        }

        private static string Recortar(string texto, int max) // corta textos largos para no desalinear el TXT
        {
            return texto.Length <= max ? texto : texto.Substring(0, max);
        }
    }
}