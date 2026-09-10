using System.Text.Json;

namespace MonopolyCore
{
    public class Mensaje
    {
        public string TipoMensaje { get; set; } = "";
        public string Accion { get; set; } = "";
        public int? JugadorId { get; set; }
        public object? Datos { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    public static class Acciones
    {
        public const string Conectar = "CONECTAR";
        public const string IniciarJuego = "INICIAR_JUEGO";
        public const string TirarDados = "TIRAR_DADOS";
        public const string ComprarPropiedad = "COMPRAR_PROPIEDAD";
        public const string NoComprar = "NO_COMPRAR";
        public const string TerminarTurno = "TERMINAR_TURNO";
        public const string ConsultarEstado = "CONSULTAR_ESTADO";
        public const string ConsultarTransacciones = "CONSULTAR_TRANSACCIONES";

        public const string RfidDetectado = "RFID_DETECTADO";
        public const string MostrarDado = "MOSTRAR_DADO";

        // Propuestas nuevas para el hardware
        public const string BotonDados = "BOTON_DADOS";
        public const string VincularRfid = "VINCULAR_RFID";
    }
}