using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using MonopolyCore.Comunicacion;

namespace MonopolyServidor.Comunicacion
{
    public class ServidorTcp
    {
        private readonly TcpListener listener;
        private readonly int puerto;

        public ServidorTcp(int puerto)
        {
            this.puerto = puerto;
            listener = new TcpListener(IPAddress.Any, puerto);
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

                    string jsonRespuesta =
                        JsonSerializer.Serialize(respuesta);

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
            return new RespuestaMensaje
            {
                TipoMensaje = TiposMensaje.Respuesta,
                Accion = mensaje.Accion,
                JugadorId = mensaje.JugadorId,
                Exito = true,
                Mensaje = "Mensaje recibido correctamente"
            };
        }
    }
}