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

        // Acciones extra de consulta
        public const string ExportarTransacciones = "EXPORTAR_TRANSACCIONES";

        // Notificaciones que el servidor manda a todos después de cada acción importante
        public const string JugadorConectado = "JUGADOR_CONECTADO";
        public const string JuegoIniciado = "JUEGO_INICIADO";
        public const string EstadoActualizado = "ESTADO_ACTUALIZADO";
        public const string JugadorMovido = "JUGADOR_MOVIDO";
        public const string CompraDisponible = "COMPRA_DISPONIBLE";
        public const string PropiedadComprada = "PROPIEDAD_COMPRADA";
        public const string PagoPendiente = "PAGO_PENDIENTE";
        public const string PagoRealizado = "PAGO_REALIZADO";
        public const string CartaTomada = "CARTA_TOMADA";
        public const string TurnoCambiado = "TURNO_CAMBIADO";
        public const string TurnoPerdido = "TURNO_PERDIDO";
        public const string JugadorEliminado = "JUGADOR_ELIMINADO";
        public const string FinJuego = "FIN_JUEGO";
        public const string MensajeJuego = "MENSAJE_JUEGO";
    }

    // Movido desde Program.cs del servidor: Juego (en Core) también necesita estos códigos
    public static class CodigosError
    {
        public const string FueraDeTurno = "FUERA_DE_TURNO";
        public const string SaldoInsuficiente = "SALDO_INSUFICIENTE";
        public const string PropiedadYaVendida = "PROPIEDAD_YA_VENDIDA";
        public const string DadosYaLanzados = "DADOS_YA_LANZADOS";
        public const string JugadorNoEncontrado = "JUGADOR_NO_ENCONTRADO";
        public const string AccionInvalida = "ACCION_INVALIDA";
        public const string JugadorEliminado = "JUGADOR_ELIMINADO";
        public const string PagoPendiente = "PAGO_PENDIENTE";
        public const string JuegoNoIniciado = "JUEGO_NO_INICIADO";
        public const string MensajeInvalido = "MENSAJE_INVALIDO";
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