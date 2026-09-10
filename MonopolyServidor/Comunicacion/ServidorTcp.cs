using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using MonopolyCore.Comunicacion;
using MonopolyCore.Modelos;

namespace MonopolyServidor.Comunicacion
{
    public class ServidorTcp
    {
        private readonly TcpListener listener;
        private readonly int puerto;
        private readonly JsonSerializerOptions opcionesJson = new JsonSerializerOptions{
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping};
        private readonly Dado dado;

        public ServidorTcp(int puerto)
        {
            this.puerto = puerto;
            listener = new TcpListener(IPAddress.Any, puerto);
            dado = new Dado();
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

                using StreamReader reader =
                    new StreamReader(stream, Encoding.UTF8);

                using StreamWriter writer =
                    new StreamWriter(stream, Encoding.UTF8)
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

                    RespuestaMensaje respuesta = ProcesarMensaje(mensaje);

                    string jsonRespuesta = JsonSerializer.Serialize(respuesta, opcionesJson);

                    await writer.WriteLineAsync(jsonRespuesta);

                    Console.WriteLine($"Enviado: {jsonRespuesta}");
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

        private RespuestaMensaje ProcesarMensaje(MensajeBase mensaje)
        {
            switch (mensaje.Accion)
            {
                case Acciones.Conectar:
                    return ProcesarConexion(mensaje);

                case Acciones.BotonPresionado:
                    return ProcesarBotonPresionado(mensaje);

                default:
                    return CrearError(
                        mensaje,
                        CodigosError.AccionInvalida,
                        $"La acción {mensaje.Accion} no es válida."
                    );
            }
        }

        private RespuestaMensaje ProcesarConexion(MensajeBase mensaje)
        {
            if (mensaje.Datos is JsonElement datos &&
                datos.ValueKind == JsonValueKind.Object &&
                datos.TryGetProperty("TipoCliente", out JsonElement tipoCliente))
            {
                if (tipoCliente.GetString() == "Hardware")
                {
                    Console.WriteLine("Hardware Raspberry conectado.");

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

            return new RespuestaMensaje
            {
                TipoMensaje = TiposMensaje.Respuesta,
                Accion = Acciones.Conectar,
                JugadorId = mensaje.JugadorId,
                Exito = true,
                Mensaje = "Cliente conectado correctamente"
            };
        }

        private RespuestaMensaje ProcesarBotonPresionado(MensajeBase mensaje)
        {
            (int valor1, int valor2) = dado.Lanzar();

            Console.WriteLine($"Dados lanzados: {valor1} y {valor2}");

            DatosDados datosDados = new DatosDados
            {
                Valor1 = valor1,
                Valor2 = valor2
            };

            return new RespuestaMensaje
            {
                TipoMensaje = TiposMensaje.Respuesta,
                Accion = Acciones.BotonPresionado,
                JugadorId = mensaje.JugadorId,
                Exito = true,
                Mensaje = "Tirada realizada correctamente",
                Datos = JsonSerializer.SerializeToElement(datosDados)
            };
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

    }
}