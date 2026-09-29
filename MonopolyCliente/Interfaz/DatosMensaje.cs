using System.Text.Json;
using MonopolyCore.Comunicacion;

namespace MonopolyCliente.Interfaz
{
    // Convierte el campo Datos de los mensajes a objetos. NO interpreta reglas del juego:
    // solo traduce el JSON que ya decidió el servidor.
    internal static class DatosMensaje
    {
        // Datos -> objeto (ej. EstadoJuegoDto). Devuelve null si no viene o no tiene la forma esperada.
        public static T? Leer<T>(JsonElement? datos) where T : class
        {
            if (datos is not JsonElement elemento || elemento.ValueKind != JsonValueKind.Object) return null;
            try
            {
                return elemento.Deserialize<T>();
            }
            catch (JsonException)
            {
                return null;
            }
        }

        // Las notificaciones del juego llegan como Datos = { "Mensaje": "...", "Detalle": {...} }
        public static string Texto(MensajeBase mensaje)
        {
            if (mensaje.Datos is JsonElement d && d.ValueKind == JsonValueKind.Object &&
                d.TryGetProperty("Mensaje", out JsonElement texto) && texto.ValueKind == JsonValueKind.String)
            {
                return texto.GetString() ?? "";
            }
            return "";
        }

        public static JsonElement? Detalle(MensajeBase mensaje)
        {
            if (mensaje.Datos is JsonElement d && d.ValueKind == JsonValueKind.Object &&
                d.TryGetProperty("Detalle", out JsonElement detalle) && detalle.ValueKind == JsonValueKind.Object)
            {
                return detalle;
            }
            return null;
        }
    }

    // Formas del campo Detalle de algunas notificaciones (mismos nombres que manda Juego.cs)
    internal class MovimientoDetalle
    {
        public int JugadorId { get; set; }
        public int Desde { get; set; }
        public int Hasta { get; set; }
        public int[] Recorrido { get; set; } = new int[0];
        public string Casilla { get; set; } = "";
    }

    internal class FinJuegoDetalle
    {
        public int? GanadorId { get; set; }
        public string? Ganador { get; set; }
        public string Motivo { get; set; } = "";
        public JugadorDto[] Ranking { get; set; } = new JugadorDto[0];
        public string? ArchivoTransacciones { get; set; }
    }

    internal class TransaccionesRespuesta
    {
        public TransaccionDto[] Transacciones { get; set; } = new TransaccionDto[0];
    }
}
