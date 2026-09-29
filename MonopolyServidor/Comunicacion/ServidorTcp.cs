using MonopolyCore;
using MonopolyCore.Comunicacion;
using MonopolyCore.Modelos;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace MonopolyServidor.Comunicacion
{
    // Servidor TCP encargado de la comunicación entre los clientes, el hardware
    // Raspberry Pi y la lógica central del juego.
    //
    // Su responsabilidad es recibir mensajes, validar su origen, delegar las
    // acciones a Juego y enviar las respuestas o notificaciones correspondientes.
    //
    // Las reglas del Monopoly y las modificaciones del estado oficial no se
    // realizan aquí; esas responsabilidades pertenecen a Juego y Banco.
    public class ServidorTcp
    {
        private readonly TcpListener listener;
        private readonly int puerto;
        private readonly JsonSerializerOptions opcionesJson = new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

        private readonly Juego juego;

        // Configuración de la partida (ajustable para la demo).
        // minJugadores: 4 para la defensa; 2 sirve para probar con menos compus.
        private readonly StreamWriter?[] conexionesJugadores = new StreamWriter?[Juego.MaxJugadores];
        private StreamWriter? hardwareWriter;

        // Se utiliza SemaphoreSlim porque el procesamiento contiene operaciones
        // asíncronas. Esto evita modificaciones simultáneas del estado y escrituras
        // concurrentes sobre las conexiones.
        private readonly SemaphoreSlim candado = new SemaphoreSlim(1, 1);

        public ServidorTcp(int puerto) : this(puerto, new Juego()) { }

        //Constructor
        public ServidorTcp(int puerto, Juego juego)
        {
            this.puerto = puerto;
            this.juego = juego;
            listener = new TcpListener(IPAddress.Any, puerto);
        }

        //Mensaje de inicio del servidor
        public async Task IniciarAsync()
        {
            listener.Start();
            Console.WriteLine($"Servidor iniciado en el puerto {puerto}");
            Console.WriteLine("Esperando conexiones...");

            while (true) // mensaje de cliente entrante
            {
                TcpClient cliente = await listener.AcceptTcpClientAsync();
                Console.WriteLine("Nuevo cliente conectado.");
                _ = AtenderClienteAsync(cliente);
            }
        }




        //-------------------------------------------------------------------------------
        //  INICIO Y ATENCIÓN DE CONEXIONES
        //-------------------------------------------------------------------------------
        private async Task AtenderClienteAsync(TcpClient cliente)
        {
            StreamWriter? writer = null;
            try
            {
                using NetworkStream stream = cliente.GetStream();
                using StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                writer = new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = true };

                while (true)
                {
                    string? linea = await reader.ReadLineAsync();
                    if (linea == null) break;
                    if (string.IsNullOrWhiteSpace(linea)) continue;

                    Console.WriteLine($"Recibido: {linea}");

                    // Un JSON malo solo genera un error, no tumba la conexión
                    MensajeBase? mensaje;
                    try
                    {
                        mensaje = JsonSerializer.Deserialize<MensajeBase>(linea, opcionesJson);
                    }
                    catch (JsonException)
                    {
                        await candado.WaitAsync();
                        try { await EnviarAsync(writer, CrearError("", null, CodigosError.MensajeInvalido, "El mensaje no es un JSON válido.")); }
                        finally { candado.Release(); }
                        continue;
                    }
                    if (mensaje == null) continue;

                    await candado.WaitAsync();
                    try
                    {
                        await ProcesarMensajeAsync(mensaje, writer);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error procesando {mensaje.Accion}: {ex}");
                        await EnviarAsync(writer, CrearError(mensaje.Accion, mensaje.JugadorId, CodigosError.AccionInvalida, "Error interno del servidor."));
                    }
                    finally
                    {
                        candado.Release();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error con cliente: {ex.Message}");
            }
            finally
            {
                await LimpiarConexionAsync(writer);
                cliente.Close();
                Console.WriteLine("Cliente desconectado.");
            }
        }




        //-------------------------------------------------------------------------------
        //  PROCESAMIENTO DE MENSAJES
        //-------------------------------------------------------------------------------

        // Centraliza el despacho de mensajes recibidos.
        // El servidor valida el origen de la solicitud y delega la lógica a Juego.
        private async Task ProcesarMensajeAsync(MensajeBase mensaje, StreamWriter writer)
        {
            string accion = mensaje.Accion;
            ResultadoAccion resultado;
            bool responder = true;

            switch (accion)
            {
                case Acciones.Conectar:
                    resultado = ProcesarConexion(mensaje, writer);
                    break;

                // Las acciones asociadas a un jugador solo se aceptan desde la conexión
                // registrada para ese mismo jugador.
                case Acciones.IniciarJuego:
                case Acciones.TirarDados:
                case Acciones.ComprarPropiedad:
                case Acciones.NoComprar:
                case Acciones.TerminarTurno:
                case Acciones.VincularRfid:
                    if (!EsConexionDelJugador(mensaje, writer))
                    {
                        await EnviarAsync(writer, CrearError(accion, mensaje.JugadorId, CodigosError.JugadorNoEncontrado,
                        "Esta conexión no corresponde a ese jugador."));
                        return;
                    }
                    int id = mensaje.JugadorId!.Value;
                    resultado = accion switch
                    {
                        Acciones.IniciarJuego => juego.IniciarJuego(id),
                        Acciones.TirarDados => juego.TirarDados(id),
                        Acciones.ComprarPropiedad => juego.ComprarPropiedad(id),
                        Acciones.NoComprar => juego.NoComprar(id),
                        Acciones.TerminarTurno => juego.TerminarTurno(id),
                        _ => await ProcesarVincularRfidAsync(id)
                    };
                    break;

                // Estas acciones son exclusivas del hardware registrado.
                // Un cliente normal no puede simular lecturas RFID ni pulsaciones del botón.
                case Acciones.RfidDetectado:
                case Acciones.BotonPresionado:
                    if (!ReferenceEquals(writer, hardwareWriter))
                    {
                        await EnviarAsync(writer, CrearError(accion, mensaje.JugadorId, CodigosError.AccionInvalida,
                            "Solo el hardware puede enviar esta acción."));
                        return;
                    }
                    if (accion == Acciones.BotonPresionado)
                    {
                        resultado = juego.TirarDadosJugadorActual();
                        responder = false; //el botón no espera respuesta
                        if (!resultado.Exito) Console.WriteLine($"Botón ignorado: {resultado.Mensaje}");
                    }
                    else
                    {
                        resultado = ProcesarRfidDetectado(mensaje);
                    }
                    break;

                //  consultas (cualquiera puede consultar) 
                case Acciones.ConsultarEstado:
                    resultado = ResultadoAccion.Ok("Estado consultado correctamente.", juego.ObtenerEstado());
                    resultado.NotificarEstado = false;
                    break;

                case Acciones.ConsultarTransacciones:
                    resultado = ProcesarConsultarTransacciones(mensaje);
                    break;

                case Acciones.ExportarTransacciones:
                    string ruta = juego.ExportarTransacciones();
                    Console.WriteLine($"Historial exportado en {ruta}");
                    resultado = ResultadoAccion.Ok($"Historial exportado en {ruta}", new { Ruta = ruta });
                    resultado.NotificarEstado = false;
                    break;


                // cualquier otra accion es un error
                default:
                    resultado = ResultadoAccion.Error(CodigosError.AccionInvalida, $"La acción {accion} no es válida.");
                    break;
            }

            if (responder)
            {
                await EnviarAsync(writer, CrearRespuesta(accion, mensaje.JugadorId, resultado));
            }

            // Propaga a los clientes y al hardware los eventos generados por Juego.
            // ServidorTcp únicamente transporta estos resultados; no genera reglas de juego.
            if (resultado.Exito)
            {
                await DifundirAsync(resultado);
            }
        }
        



        //-------------------------------------------------------------------------------
        //  CONEXIÓN Y REGISTRO
        //-------------------------------------------------------------------------------
        private ResultadoAccion ProcesarConexion(MensajeBase mensaje, StreamWriter writer)
        {
            // ¿Es la Raspberry?
            if (mensaje.Datos is JsonElement datos && datos.ValueKind == JsonValueKind.Object &&
                datos.TryGetProperty("TipoCliente", out JsonElement tipoCliente) &&
                tipoCliente.ValueKind == JsonValueKind.String && tipoCliente.GetString() == "Hardware")
            {
                hardwareWriter = writer;
                juego.UsarRfidParaPagos = true;
                Console.WriteLine("Hardware Raspberry registrado.");
                ResultadoAccion r = ResultadoAccion.Ok("Hardware conectado correctamente");
                r.NotificarEstado = true;
                return r;
            }

            // Reconexión de un jugador ya registrado
            if (mensaje.JugadorId.HasValue)
            {
                int id = mensaje.JugadorId.Value;
                Jugador? jugador = juego.ObtenerJugador(id);
                if (jugador == null)
                    return ResultadoAccion.Error(CodigosError.JugadorNoEncontrado, "El jugador indicado no está registrado.");

                conexionesJugadores[id - 1] = writer;
                Console.WriteLine($"Jugador reconectado: {jugador.nombre} - ID: {id}");
                ResultadoAccion r = ResultadoAccion.Ok($"Jugador {jugador.nombre} reconectado correctamente.");
                r.JugadorId = id;
                r.NotificarEstado = false;
                return r;
            }

            // Jugador nuevo
            string nombre = "";
            if (mensaje.Datos is JsonElement d && d.ValueKind == JsonValueKind.Object &&
                d.TryGetProperty("Nombre", out JsonElement nombreElemento) && nombreElemento.ValueKind == JsonValueKind.String)
            {
                nombre = nombreElemento.GetString() ?? "";
            }

            ResultadoAccion resultado = juego.AgregarJugador(nombre);
            if (resultado.Exito && resultado.JugadorId.HasValue)
            {
                conexionesJugadores[resultado.JugadorId.Value - 1] = writer;
                Console.WriteLine($"Jugador registrado: {nombre} - ID: {resultado.JugadorId}");
            }
            return resultado;
        }

        // El JugadorId del mensaje debe corresponder a la conexión que lo registró.
        private bool EsConexionDelJugador(MensajeBase mensaje, StreamWriter writer)
        {
            if (!mensaje.JugadorId.HasValue) return false;
            int id = mensaje.JugadorId.Value;
            if (id < 1 || id > Juego.MaxJugadores) return false;
            return ReferenceEquals(conexionesJugadores[id - 1], writer);
        }

        private async Task LimpiarConexionAsync(StreamWriter? writer)
        {
            if (writer == null) return;

            await candado.WaitAsync();
            try
            {
                for (int i = 0; i < conexionesJugadores.Length; i++)
                {
                    if (ReferenceEquals(conexionesJugadores[i], writer))
                    {
                        conexionesJugadores[i] = null;
                        Console.WriteLine($"Jugador {i + 1} sin conexión (puede reconectarse con su ID).");
                    }
                }

                if (ReferenceEquals(hardwareWriter, writer))
                {
                    hardwareWriter = null;
                    Console.WriteLine("Se perdió la conexión con el hardware: pagos sin RFID.");
                    ResultadoAccion r = juego.DesactivarRfid();
                    await DifundirAsync(r);
                }
            }
            finally
            {
                candado.Release();
            }
        }



        //-------------------------------------------------------------------------------
        //  RFID Y HARDWARE
        //-------------------------------------------------------------------------------
        private async Task<ResultadoAccion> ProcesarVincularRfidAsync(int jugadorId)
        {
            if (hardwareWriter == null) //si no hay hardware no se procesa RFID
                return ResultadoAccion.Error(CodigosError.AccionInvalida, "No hay hardware RFID conectado.");

            ResultadoAccion resultado = juego.SolicitarVinculacionRfid(jugadorId);
            if (!resultado.Exito) return resultado;

            bool enviado = await EnviarAHardwareAsync(Acciones.EsperarRfid, jugadorId, null);
            if (!enviado)
            {
                juego.CancelarVinculacionRfid();
                return ResultadoAccion.Error(CodigosError.AccionInvalida, "No se pudo comunicar con el hardware RFID.");
            }
            return resultado;
        }

        private ResultadoAccion ProcesarRfidDetectado(MensajeBase mensaje)
        {
            if (mensaje.Datos is not JsonElement datos || datos.ValueKind != JsonValueKind.Object)
                return ResultadoAccion.Error(CodigosError.AccionInvalida, "No se recibieron datos del RFID.");

            DatosRfid? datosRfid = JsonSerializer.Deserialize<DatosRfid>(datos.GetRawText());
            if (datosRfid == null || string.IsNullOrWhiteSpace(datosRfid.UID))
                return ResultadoAccion.Error(CodigosError.AccionInvalida, "El UID recibido no es válido.");

            Console.WriteLine($"RFID detectado: {datosRfid.UID}");
            return juego.ProcesarRfid(datosRfid.UID);
        }
        


        //-------------------------------------------------------------------------------
        //  CONSULTAS
        //-------------------------------------------------------------------------------

        // Datos opcionales: { "FiltroJugadorId": 2, "Tipo": "PagoAlquiler", "DesdeInicio": false }
        private ResultadoAccion ProcesarConsultarTransacciones(MensajeBase mensaje)
        {
            int? filtroJugador = null;
            TipoTransaccion? filtroTipo = null;
            bool desdeInicio = true;

            if (mensaje.Datos is JsonElement datos && datos.ValueKind == JsonValueKind.Object)
            {
                if (datos.TryGetProperty("FiltroJugadorId", out JsonElement j) && j.ValueKind == JsonValueKind.Number)filtroJugador = j.GetInt32();

                if (datos.TryGetProperty("Tipo", out JsonElement t) && t.ValueKind == JsonValueKind.String)
                {
                    if (!Enum.TryParse(t.GetString(), true, out TipoTransaccion tipo))
                        return ResultadoAccion.Error(CodigosError.AccionInvalida, $"Tipo de transacción desconocido: {t.GetString()}");
                    filtroTipo = tipo;
                }

                if (datos.TryGetProperty("DesdeInicio", out JsonElement o) &&
                    (o.ValueKind == JsonValueKind.True || o.ValueKind == JsonValueKind.False))
                    desdeInicio = o.GetBoolean();
            }

            TransaccionDto[] transacciones = juego.ConsultarTransacciones(filtroJugador, filtroTipo, desdeInicio);
            ResultadoAccion r = ResultadoAccion.Ok($"{transacciones.Length} transacciones.", new { Transacciones = transacciones });
            r.NotificarEstado = false;
            return r;
        }



        //-------------------------------------------------------------------------------
        //  ENVÍO Y NOTIFICACIONES
        //-------------------------------------------------------------------------------

        // Después de cada acción exitosa: eventos -> todos, dados/pagos -> hardware, estado -> todos.
        private async Task DifundirAsync(ResultadoAccion resultado)
        {
            if (resultado.Dados != null) //primero el dado físico
            {
                await EnviarAHardwareAsync(Acciones.MostrarDado, null, resultado.Dados);
            }

            foreach (EventoJuego evento in resultado.Eventos)
            {
                MensajeBase notificacion = new MensajeBase
                {
                    TipoMensaje = TiposMensaje.Notificacion,
                    Accion = evento.Accion,
                    JugadorId = evento.JugadorId,
                    Datos = JsonSerializer.SerializeToElement(new { evento.Mensaje, Detalle = evento.Datos }, opcionesJson)
                };
                await EnviarATodosAsync(notificacion);

                // El hardware espera la tarjeta cuando alguien tiene que pagar
                if (evento.Accion == Acciones.PagoPendiente)
                {
                    await EnviarAHardwareAsync(Acciones.EsperarRfid, evento.JugadorId, null);
                }
            }

            if (resultado.NotificarEstado)
            {
                await EnviarATodosAsync(new MensajeBase
                {
                    TipoMensaje = TiposMensaje.Notificacion,
                    Accion = Acciones.EstadoActualizado,
                    Datos = JsonSerializer.SerializeToElement(juego.ObtenerEstado(), opcionesJson)
                });
            }
        }


        private async Task EnviarATodosAsync(MensajeBase mensaje)
        {
            for (int i = 0; i < conexionesJugadores.Length; i++)
            {
                StreamWriter? writer = conexionesJugadores[i];
                if (writer == null) continue;
                if (!await EnviarAsync(writer, mensaje))
                {
                    conexionesJugadores[i] = null;
                    Console.WriteLine($"No se pudo notificar al jugador {i + 1}.");
                }
            }
        }

        // Pantilla para enviar mensajes al hardware
        private async Task<bool> EnviarAHardwareAsync(string accion, int? jugadorId, object? datos)
        {
            if (hardwareWriter == null) return false;

            MensajeBase mensaje = new MensajeBase
            {
                TipoMensaje = TiposMensaje.Notificacion,
                Accion = accion,
                JugadorId = jugadorId,
                Datos = datos != null ? JsonSerializer.SerializeToElement(datos, opcionesJson) : null
            };

            if (await EnviarAsync(hardwareWriter, mensaje)) return true;//si no contesta, no hay hardware

            hardwareWriter = null;
            Console.WriteLine("Se perdió la conexión con el hardware.");
            return false;
        }

        private async Task<bool> EnviarAsync(StreamWriter writer, MensajeBase mensaje)
        {
            try
            {
                string json = JsonSerializer.Serialize(mensaje, mensaje.GetType(), opcionesJson);
                await writer.WriteLineAsync(json);
                Console.WriteLine($"Enviado: {json}");
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Pantilla para respuestas
        private RespuestaMensaje CrearRespuesta(string accion, int? jugadorId, ResultadoAccion resultado)
        {
            if (!resultado.Exito)
                return CrearError(accion, jugadorId, resultado.Codigo, resultado.Mensaje);

            return new RespuestaMensaje
            {
                TipoMensaje = TiposMensaje.Respuesta,
                Accion = accion,
                JugadorId = resultado.JugadorId ?? jugadorId,
                Exito = true,
                Mensaje = resultado.Mensaje,
                Datos = resultado.Datos != null ? JsonSerializer.SerializeToElement(resultado.Datos, resultado.Datos.GetType(), opcionesJson) : null
            };
        }

        // Pantilla para mensajes de error
        private RespuestaMensaje CrearError(string accion, int? jugadorId, string codigo, string descripcion)
        {
            return new RespuestaMensaje
            {
                TipoMensaje = TiposMensaje.Respuesta,
                Accion = accion,
                JugadorId = jugadorId,
                Exito = false,
                Mensaje = descripcion,
                Datos = JsonSerializer.SerializeToElement(new { Codigo = codigo })
            };
        }
    }
}
