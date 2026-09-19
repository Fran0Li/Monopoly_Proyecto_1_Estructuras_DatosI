using MonopolyCore.Comunicacion;
using MonopolyCore.Estructuras;
using MonopolyCore.Modelos;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace MonopolyServidor.Comunicacion
{
    public class ServidorTcp
    {
        private readonly TcpListener listener;
        private readonly int puerto;
        private readonly JsonSerializerOptions opcionesJson = new JsonSerializerOptions{
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping};
        private readonly Dado dado;
        private const int MaxJugadores = 4;

        private readonly Jugador?[] jugadoresRegistrados;
        private readonly StreamWriter?[] conexionesJugadores;

        private readonly ColaCircular<Jugador> turnos;

        private int cantidadJugadores;
        private bool turnosInicializados;

        private readonly Random random;

        private StreamWriter? hardwareWriter;

        private int? jugadorPendienteRfid;

        public ServidorTcp(int puerto)
        {
            this.puerto = puerto;
            listener = new TcpListener(IPAddress.Any, puerto);

            dado = new Dado();

            jugadoresRegistrados = new Jugador?[MaxJugadores];
            conexionesJugadores = new StreamWriter?[MaxJugadores];

            turnos = new ColaCircular<Jugador>();

            cantidadJugadores = 0;
            turnosInicializados = false;

            random = new Random();

            jugadorPendienteRfid = null;
        }

        public async Task IniciarAsync()
        {
            listener.Start();

            Console.WriteLine($"Servidor iniciado en el puerto {puerto}");
            Console.WriteLine("Esperando conexiones...");

            while (true)
            {
                TcpClient cliente = await listener.AcceptTcpClientAsync();

                Console.WriteLine("Nuevo cliente conectado.");

                _ = AtenderClienteAsync(cliente);
            }
        }

        private async Task AtenderClienteAsync(TcpClient cliente)
        {
            try
            {
                using NetworkStream stream = cliente.GetStream();

                using StreamReader reader = new StreamReader(stream, Encoding.UTF8);

                using StreamWriter writer = new StreamWriter(stream, Encoding.UTF8)
                    {
                        AutoFlush = true
                    };

                while (cliente.Connected)
                {
                    string? linea = await reader.ReadLineAsync();

                    if (linea == null)
                    {
                        break;
                    }

                    Console.WriteLine($"Recibido: {linea}");

                    MensajeBase? mensaje =
                        JsonSerializer.Deserialize<MensajeBase>(linea);

                    if (mensaje == null)
                    {
                        continue;
                    }

                    RespuestaMensaje? respuesta =await ProcesarMensajeAsync(mensaje, writer);

                    if (respuesta != null)
                    {
                        string jsonRespuesta =
                            JsonSerializer.Serialize(respuesta, opcionesJson);

                        await writer.WriteLineAsync(jsonRespuesta);

                        Console.WriteLine($"Enviado: {jsonRespuesta}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error con cliente: {ex.Message}");
            }
            finally
            {
                cliente.Close();
                Console.WriteLine("Cliente desconectado.");
            }
        }

        private async Task<RespuestaMensaje?> ProcesarMensajeAsync(MensajeBase mensaje,StreamWriter writer)
        {
            switch (mensaje.Accion)
            {
                case Acciones.Conectar:
                    return ProcesarConexion(mensaje, writer);

                case Acciones.VincularRfid:
                    return await ProcesarVincularRfidAsync(mensaje);

                case Acciones.RfidDetectado:
                    return await ProcesarRfidDetectadoAsync(mensaje);

                case Acciones.BotonPresionado:
                    await ProcesarBotonPresionadoAsync();
                    return null;

                default:
                    return CrearError(
                        mensaje,
                        CodigosError.AccionInvalida,
                        $"La acción {mensaje.Accion} no es válida."
                    );
            }
        }

        private RespuestaMensaje ProcesarConexion(MensajeBase mensaje,StreamWriter writer)
        {
            // ¿Es la Raspberry?
            if (mensaje.Datos is JsonElement datos &&
                datos.ValueKind == JsonValueKind.Object &&
                datos.TryGetProperty("TipoCliente", out JsonElement tipoCliente))
            {
                if (tipoCliente.GetString() == "Hardware")
                {
                    hardwareWriter = writer;

                    Console.WriteLine("Hardware Raspberry registrado.");

                    return new RespuestaMensaje
                    {
                        TipoMensaje = TiposMensaje.Respuesta,
                        Accion = Acciones.Conectar,
                        JugadorId = null,
                        Exito = true,
                        Mensaje = "Hardware conectado correctamente"
                    };
                }
            }

            // Si trae JugadorId, intentamos reconectarlo
            if (mensaje.JugadorId.HasValue)
            {
                return ProcesarReconexionJugador(
                    mensaje.JugadorId.Value,
                    writer
                );
            }

            // Si no trae ID es un jugador nuevo
            return RegistrarJugador(mensaje, writer);
        }

        private RespuestaMensaje RegistrarJugador(MensajeBase mensaje,StreamWriter writer)
        {
            if (cantidadJugadores >= MaxJugadores)
            {
                return CrearError(
                    mensaje,
                    CodigosError.AccionInvalida,
                    "La partida ya tiene 4 jugadores."
                );
            }

            if (mensaje.Datos is not JsonElement datos || !datos.TryGetProperty("Nombre", out JsonElement nombreElemento))
            {
                return CrearError(
                    mensaje,
                    CodigosError.AccionInvalida,
                    "No se recibió el nombre del jugador."
                );
            }

            string? nombre = nombreElemento.GetString();

            if (string.IsNullOrWhiteSpace(nombre))
            {
                return CrearError(
                    mensaje,
                    CodigosError.AccionInvalida,
                    "El nombre del jugador no es válido."
                );
            }

            int nuevoId = cantidadJugadores + 1;

            Jugador jugador = new Jugador(nuevoId, nombre);

            jugadoresRegistrados[nuevoId - 1] = jugador;
            conexionesJugadores[nuevoId - 1] = writer;

            cantidadJugadores++;

            Console.WriteLine($"Jugador registrado: {nombre} - ID: {nuevoId}");

            if (cantidadJugadores == MaxJugadores)
            {
                RifarlosYCrearTurnos();
            }

            return new RespuestaMensaje
            {
                TipoMensaje = TiposMensaje.Respuesta,
                Accion = Acciones.Conectar,
                JugadorId = nuevoId,
                Exito = true,
                Mensaje = $"Jugador {nombre} registrado correctamente. ID: {nuevoId}"
            };
        }

        private RespuestaMensaje ProcesarReconexionJugador(int jugadorId,StreamWriter writer)
        {
            if (jugadorId < 1 || jugadorId > MaxJugadores)
            {
                return new RespuestaMensaje
                {
                    TipoMensaje = TiposMensaje.Respuesta,
                    Accion = Acciones.Conectar,
                    JugadorId = jugadorId,
                    Exito = false,
                    Mensaje = "El jugador indicado no existe."
                };
            }

            Jugador? jugador = jugadoresRegistrados[jugadorId - 1];

            if (jugador == null)
            {
                return new RespuestaMensaje
                {
                    TipoMensaje = TiposMensaje.Respuesta,
                    Accion = Acciones.Conectar,
                    JugadorId = jugadorId,
                    Exito = false,
                    Mensaje = "El jugador indicado no está registrado."
                };
            }

            // Sustituimos el socket viejo por el nuevo
            conexionesJugadores[jugadorId - 1] = writer;

            Console.WriteLine($"Jugador reconectado: {jugador.Nombre} - ID: {jugadorId}");

            return new RespuestaMensaje
            {
                TipoMensaje = TiposMensaje.Respuesta,
                Accion = Acciones.Conectar,
                JugadorId = jugadorId,
                Exito = true,
                Mensaje = $"Jugador {jugador.Nombre} reconectado correctamente."
            };
        }

        private void RifarlosYCrearTurnos()
        {
            if (turnosInicializados)
            {
                return;
            }

            Jugador[] orden = new Jugador[MaxJugadores];

            for (int i = 0; i < MaxJugadores; i++)
            {
                orden[i] = jugadoresRegistrados[i]!;
            }

            // Fisher-Yates
            for (int i = orden.Length - 1; i > 0; i--)
            {
                int posicionAleatoria = random.Next(i + 1);

                Jugador temporal = orden[i];

                orden[i] = orden[posicionAleatoria];
                orden[posicionAleatoria] = temporal;
            }

            Console.WriteLine();
            Console.WriteLine("=== ORDEN DE TURNOS ===");

            for (int i = 0; i < orden.Length; i++)
            {
                turnos.Encolar(orden[i]);

                Console.WriteLine(
                    $"{i + 1}. {orden[i].Nombre}"
                );
            }

            turnosInicializados = true;

            Console.WriteLine("=======================");
            Console.WriteLine();
        }

        private async Task<RespuestaMensaje> ProcesarVincularRfidAsync(MensajeBase mensaje)
        {
            if (!mensaje.JugadorId.HasValue)
            {
                return CrearError(
                    mensaje,
                    CodigosError.JugadorNoEncontrado,
                    "No se recibió el ID del jugador."
                );
            }

            int jugadorId = mensaje.JugadorId.Value;

            if (jugadorId < 1 || jugadorId > MaxJugadores)
            {
                return CrearError(
                    mensaje,
                    CodigosError.JugadorNoEncontrado,
                    "El jugador indicado no existe."
                );
            }

            Jugador? jugador = jugadoresRegistrados[jugadorId - 1];

            if (jugador == null)
            {
                return CrearError(
                    mensaje,
                    CodigosError.JugadorNoEncontrado,
                    "El jugador no está registrado."
                );
            }

            if (hardwareWriter == null)
            {
                return CrearError(
                    mensaje,
                    CodigosError.AccionInvalida,
                    "No hay hardware RFID conectado."
                );
            }

            if (jugadorPendienteRfid.HasValue)
            {
                return CrearError(
                    mensaje,
                    CodigosError.AccionInvalida,
                    "Ya existe un jugador esperando vincular una tarjeta RFID."
                );
            }

            jugadorPendienteRfid = jugadorId;

            await EnviarEsperarRfidHardwareAsync(jugadorId);

            Console.WriteLine(
                $"Jugador {jugador.Nombre} esperando vinculación RFID."
            );

            return new RespuestaMensaje
            {
                TipoMensaje = TiposMensaje.Respuesta,
                Accion = Acciones.VincularRfid,
                JugadorId = jugadorId,
                Exito = true,
                Mensaje = "Acerque una tarjeta al lector RFID."
            };
        }

        private async Task EnviarEsperarRfidHardwareAsync(int jugadorId)
        {
            if (hardwareWriter == null)
            {
                return;
            }

            MensajeBase mensaje = new MensajeBase
            {
                TipoMensaje = TiposMensaje.Notificacion,
                Accion = Acciones.EsperarRfid,
                JugadorId = jugadorId
            };

            string json = JsonSerializer.Serialize(mensaje, opcionesJson);

            await hardwareWriter.WriteLineAsync(json);

            Console.WriteLine($"Servidor esperando RFID para jugador {jugadorId}.");
        }

        private async Task<RespuestaMensaje> ProcesarRfidDetectadoAsync(MensajeBase mensaje)
        {
            if (mensaje.Datos is not JsonElement datos)
            {
                return CrearError(
                    mensaje,
                    CodigosError.AccionInvalida,
                    "No se recibieron datos del RFID."
                );
            }

            DatosRfid? datosRfid = JsonSerializer.Deserialize<DatosRfid>(datos.GetRawText());

            if (datosRfid == null || string.IsNullOrWhiteSpace(datosRfid.UID))
            {
                return CrearError(
                    mensaje,
                    CodigosError.AccionInvalida,
                    "El UID recibido no es válido."
                );
            }

            string uid = datosRfid.UID;

            Console.WriteLine($"RFID detectado: {uid}");

            // Si nadie está intentando vincular una tarjeta,
            // solamente reconocemos el RFID.
            if (!jugadorPendienteRfid.HasValue)
            {
                return new RespuestaMensaje
                {
                    TipoMensaje = TiposMensaje.Respuesta,
                    Accion = Acciones.RfidDetectado,
                    JugadorId = null,
                    Exito = true,
                    Mensaje = $"RFID detectado: {uid}"
                };
            }

            // Verificar que esa tarjeta no pertenezca ya
            // a otro jugador.
            for (int i = 0; i < cantidadJugadores; i++)
            {
                Jugador? registrado = jugadoresRegistrados[i];

                if (registrado != null && registrado.TarjetaRfid == uid && registrado != jugadoresRegistrados[jugadorPendienteRfid.Value - 1])
                {
                    return CrearError(
                        mensaje,
                        CodigosError.AccionInvalida,
                        "Esta tarjeta RFID ya está vinculada a otro jugador."
                    );
                }
            }

            int jugadorId = jugadorPendienteRfid.Value;

            Jugador? jugador = jugadoresRegistrados[jugadorId - 1];

            if (jugador == null)
            {
                jugadorPendienteRfid = null;

                return CrearError(
                    mensaje,
                    CodigosError.JugadorNoEncontrado,
                    "No se encontró el jugador pendiente de vinculación."
                );
            }

            // Vinculación.
            jugador.TarjetaRfid = uid;

            jugadorPendienteRfid = null;

            Console.WriteLine($"RFID {uid} vinculado al jugador " + $"{jugador.Nombre} (ID {jugadorId}).");

            await NotificarRfidVinculadoAsync(jugadorId,uid);

            return new RespuestaMensaje
            {
                TipoMensaje = TiposMensaje.Respuesta,
                Accion = Acciones.RfidDetectado,
                JugadorId = jugadorId,
                Exito = true,
                Mensaje = $"RFID vinculado correctamente al jugador {jugador.Nombre}."
            };
        }

        private async Task NotificarRfidVinculadoAsync(int jugadorId,string uid)
        {
            StreamWriter? writer = conexionesJugadores[jugadorId - 1];

            if (writer == null)
            {
                return;
            }

            DatosRfid datosRfid = new DatosRfid
            {
                UID = uid
            };

            MensajeBase notificacion = new MensajeBase
            {
                TipoMensaje = TiposMensaje.Notificacion,
                Accion = Acciones.RfidVinculado,
                JugadorId = jugadorId,
                Datos = JsonSerializer.SerializeToElement(datosRfid)
            };

            string json = JsonSerializer.Serialize(notificacion,opcionesJson);

            await writer.WriteLineAsync(json);

            Console.WriteLine($"Jugador {jugadorId} notificado de su RFID.");}

        private async Task ProcesarBotonPresionadoAsync()
        {
            (int valor1, int valor2) = dado.Lanzar();

            Console.WriteLine($"Dados lanzados: {valor1} y {valor2}");

            await EnviarDadosHardwareAsync(valor1, valor2);
        }

        private RespuestaMensaje CrearError(MensajeBase mensaje,string codigo,string descripcion)
        {
            return new RespuestaMensaje
            {
                TipoMensaje = TiposMensaje.Respuesta,
                Accion = mensaje.Accion,
                JugadorId = mensaje.JugadorId,
                Exito = false,
                Mensaje = descripcion,
                Datos = JsonSerializer.SerializeToElement(new{Codigo = codigo})
            };
        }

        private async Task EnviarDadosHardwareAsync(int valor1, int valor2)
        {
            if (hardwareWriter == null)
            {
                Console.WriteLine("No hay hardware conectado.");
                return;
            }

            DatosDados datosDados = new DatosDados
            {
                Valor1 = valor1,
                Valor2 = valor2
            };

            MensajeBase notificacion = new MensajeBase
            {
                TipoMensaje = TiposMensaje.Notificacion,
                Accion = Acciones.MostrarDado,
                JugadorId = null,
                Datos = JsonSerializer.SerializeToElement(datosDados)
            };

            string json =
                JsonSerializer.Serialize(notificacion, opcionesJson);

            await hardwareWriter.WriteLineAsync(json);

            Console.WriteLine($"Enviado a Raspberry: {json}");
        }

    }
}