using System.Text.Json;

namespace MonopolyCore.Comunicacion
{
    //Se usan constantes para facilitar las validaciones y evitar errores al escribir los valores
    public static class TiposMensaje
    {
        //Tipos de mensajes
        public const string Peticion = "Peticion";
        public const string Respuesta = "Respuesta";
        public const string Notificacion = "Notificacion";
    }

    public static class Acciones
    {
        // Acciones generales
        public const string Conectar = "CONECTAR";
        public const string IniciarJuego = "INICIAR_JUEGO";
        public const string TirarDados = "TIRAR_DADOS";
        public const string ComprarPropiedad = "COMPRAR_PROPIEDAD";
        public const string NoComprar = "NO_COMPRAR";
        public const string TerminarTurno = "TERMINAR_TURNO";
        public const string ConsultarEstado = "CONSULTAR_ESTADO";
        public const string ConsultarTransacciones = "CONSULTAR_TRANSACCIONES";

        // Acciones del hardware
        public const string RfidDetectado = "RFID_DETECTADO";
        public const string MostrarDado = "MOSTRAR_DADO";
        public const string BotonPresionado = "BOTON_PRESIONADO";
        public const string VincularRfid = "VINCULAR_RFID";
        public const string EsperarRfid = "ESPERAR_RFID";
        public const string RfidVinculado = "RFID_VINCULADO";
    }

    public class MensajeBase
    {
        //Datos comunes que contiene cualquier mensaje del protocolo
        public string TipoMensaje { get; set; } = "";
        public string Accion { get; set; } = "";
        public int? JugadorId { get; set; }
        public JsonElement? Datos { get; set; } //Contiene los datos específicos de cada acción en formato JSON
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    public class RespuestaMensaje : MensajeBase //Hereda los metodos de MensajeBase
    {
        //Datos adicionales de una respuesta del servidor
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = "";
    }
}