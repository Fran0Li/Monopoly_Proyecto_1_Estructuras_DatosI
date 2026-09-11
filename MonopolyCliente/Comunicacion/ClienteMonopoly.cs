using System;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MonopolyCore.Comunicacion;

namespace MonopolyCliente.Comunicacion
{

    /// Capa de comunicación TCP del cliente WinForms.
    ///
    /// Regla de oro del proyecto: esta clase NO contiene lógica de juego.
    /// Solo hace tres cosas:
    ///   1. Abre y mantiene la conexión TCP con el servidor.
    ///   2. Empaqueta lo que el usuario quiere hacer en un MensajeBase
    ///      (siguiendo el protocolo ya definido en MonopolyCore.Comunicacion)
    ///      y lo envía.
    ///   3. Recibe lo que manda el servidor (Respuesta o Notificacion) y lo
    ///      entrega a quien esté escuchando, mediante eventos.
    ///
    /// Los formularios (Form1, FormLogin, FormTablero, etc.) se suscriben a
    /// esos eventos y ahí deciden qué mostrar en pantalla — pero ni ellos ni
    /// esta clase deciden si una jugada es válida: esa decisión es siempre
    /// del servidor. Ver ServidorTcp.ProcesarMensajeAsync del lado servidor.*/
    public class ClienteMonopoly
    {
        // Recursos de la conexión TCP. Son nullable porque no existen hasta
        // que se llama a ConectarAsync; antes de eso, "no hay conexión".
        private TcpClient? cliente;
        private NetworkStream? stream;
        private StreamReader? reader;
        private StreamWriter? writer;

        // Mismas opciones de serialización que usa el servidor (ServidorTcp.cs),
        // para que ambos lados escriban/lean el JSON de la misma forma
        // (ej. sin escapar de más ciertos caracteres).
        private readonly JsonSerializerOptions opcionesJson = new JsonSerializerOptions
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        /// Id que el servidor asigna a este jugador al conectarse. Es null
        /// hasta que llega la respuesta de CONECTAR con Exito = true.
        /// Se usa automáticamente en cada petición siguiente (ver
        /// EnviarPeticionAsync) para que el servidor sepa quién pide qué.
        
        public int? JugadorId { get; private set; }

        ///True mientras el socket siga abierto y conectado.</summary>
        public bool Conectado => cliente?.Connected ?? false;

        // Eventos: el único canal de salida hacia la GUI 
        // La GUI se suscribe a estos (+=) y decide qué pintar. Esta clase
        // solo avisa "esto llegó", nunca "esto significa que ganaste" o
        // "esto significa que no podés comprar" — esa interpretación de
        // reglas tampoco le toca al formulario, el servidor ya la resolvió
        // y la mandó en el propio mensaje (Exito, Mensaje, Datos).

        /// Se dispara cuando llega una Respuesta (contestación directa a una Peticion nuestra).</summary>
        public event Action<RespuestaMensaje>? RespuestaRecibida;

        /// Se dispara cuando llega una Notificacion (el servidor avisa algo sin que se lo pidiéramos, ej. jugada de otro jugador).</summary>
        public event Action<MensajeBase>? NotificacionRecibida;

        /// Se dispara si el loop de lectura revienta por un error de red inesperado.</summary>
        public event Action<Exception>? ErrorDeConexion;

        /// Se dispara cuando el servidor cierra la conexión (o se cae la red).</summary>
        public event Action? Desconectado;

        /// Abre el socket TCP contra el servidor, deja todo listo para
        /// leer/escribir línea por línea 
        /// y manda la primera petición: CONECTAR con el nombre
        /// del jugador.
        /// <param name="ip">IP o hostname del servidor (ej. la de la compu que hace de banco).</param>
        /// <param name="puerto">Puerto TCP donde escucha ServidorTcp.</param>
        /// <param name="nombreJugador">Nombre que el jugador escribió en el login.</param>
        public async Task ConectarAsync(string ip, int puerto, string nombreJugador)
        {
            cliente = new TcpClient();
            await cliente.ConnectAsync(ip, puerto);

            stream = cliente.GetStream();

            // StreamReader/StreamWriter sobre el NetworkStream son los que
            // resuelven el framing por nosotros: ReadLineAsync() espera a
            // que llegue un \n completo, aunque el TCP lo entregue partido
            // en varios paquetes. Por eso el protocolo exige NDJSON.
            reader = new StreamReader(stream, Encoding.UTF8);
            writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

            // El loop de lectura corre en paralelo (fire-and-forget) porque
            // el servidor puede mandarnos Notificaciones en cualquier
            // momento, no solo como respuesta a algo que pedimos nosotros.
            // No se espera (await) este Task a propósito.
            _ = EscucharServidorAsync();

            // Todavía no tenemos JugadorId asignado, así que esta primera
            // petición sale con JugadorId = null. El servidor la usa para
            // identificarnos como jugador humano (no Hardware) y, cuando
            // esté completo, nos debería devolver un id en la respuesta.
            await EnviarPeticionAsync(Acciones.Conectar, new { Nombre = nombreJugador });
        }

        /// Único punto por el que un formulario le pide algo al servidor.
        /// Arma el sobre del protocolo (MensajeBase) y lo manda como una
        /// línea de JSON terminada en \n. No valida nada: si la acción no
        /// tiene sentido (ej. comprar sin plata), el servidor responde con
        /// Exito = false y un código de error — eso lo recibe la GUI vía
        /// RespuestaRecibida y ahí decide cómo mostrarlo (MessageBox, etc.).
        public async Task EnviarPeticionAsync(string accion, object? datos = null)
        {
            if (writer == null)
                throw new InvalidOperationException("No hay conexión activa con el servidor.");

            var peticion = new MensajeBase
            {
                TipoMensaje = TiposMensaje.Peticion,
                Accion = accion,
                JugadorId = JugadorId,
                // Serializamos "datos" (un objeto anónimo cualquiera) a
                // JsonElement porque MensajeBase.Datos está tipado así,
                // igual que del lado del servidor.
                Datos = datos != null
                    ? JsonSerializer.SerializeToElement(datos, opcionesJson)
                    : null,
                Timestamp = DateTime.Now
            };

            string json = JsonSerializer.Serialize(peticion, opcionesJson);
            await writer.WriteLineAsync(json);
        }


        /// Loop que corre todo el tiempo que dure la conexión: lee una
        /// línea, la procesa, y vuelve a esperar la siguiente. Termina solo
        /// cuando ReadLineAsync devuelve null (servidor cerró el socket) o
        /// cuando algo revienta (red caída, etc.).
        private async Task EscucharServidorAsync()
        {
            try
            {
                while (cliente != null && cliente.Connected)
                {
                    string? linea = await reader!.ReadLineAsync();

                    if (linea == null)
                        break; // el servidor cerró la conexión de su lado

                    ProcesarLinea(linea);
                }
            }
            catch (Exception ex)
            {
                // Cualquier excepción de red (ej. IOException por cable
                // desconectado) se convierte en evento en vez de tumbar
                // la app; la GUI decide si muestra un aviso o reintenta.
                ErrorDeConexion?.Invoke(ex);
            }
            finally
            {
                Desconectado?.Invoke();
            }
        }

        /// Decide si una línea recibida es una Respuesta o una Notificacion
        /// y la reparte por el evento correspondiente. Se mira el campo
        /// "TipoMensaje" primero (con JsonDocument, sin deserializar todo
        /// todavía) porque RespuestaMensaje y MensajeBase no tienen la
        /// misma forma (Respuesta agrega Exito y Mensaje).
        private void ProcesarLinea(string linea)
        {
            using JsonDocument doc = JsonDocument.Parse(linea);
            string tipoMensaje = doc.RootElement.GetProperty("TipoMensaje").GetString() ?? "";

            if (tipoMensaje == TiposMensaje.Respuesta)
            {
                RespuestaMensaje? respuesta = JsonSerializer.Deserialize<RespuestaMensaje>(linea, opcionesJson);
                if (respuesta == null) return;

                // Si es la confirmación de CONECTAR y salió bien, guardamos
                // el JugadorId para usarlo automáticamente en las próximas
                // peticiones. OJO: el servidor todavía no asigna un id real
                // acá (ver nota en el chat) — por ahora esto puede seguir
                // guardando null, y está bien así hasta que se complete
                // ProcesarConexion del lado servidor.
                if (respuesta.Accion == Acciones.Conectar && respuesta.Exito && respuesta.JugadorId.HasValue)
                {
                    JugadorId = respuesta.JugadorId;
                }

                RespuestaRecibida?.Invoke(respuesta);
            }
            else if (tipoMensaje == TiposMensaje.Notificacion)
            {
                MensajeBase? notificacion = JsonSerializer.Deserialize<MensajeBase>(linea, opcionesJson);
                if (notificacion != null)
                    NotificacionRecibida?.Invoke(notificacion);
            }
            // Si llega un TipoMensaje distinto a estos dos, se ignora: no
            // debería pasar según el protocolo, pero no vale la pena tumbar
            // el loop de lectura por un mensaje raro.
        }

        /// Cierra la conexión de forma ordenada
        ///  Libera reader/writer/stream/socket.
        public void Desconectar()
        {
            writer?.Dispose();
            reader?.Dispose();
            stream?.Dispose();
            cliente?.Close();
        }
    }
}
