using MonopolyCore.Comunicacion;
using MonopolyCore.Estructuras;

namespace MonopolyCore
{
    // Algo que pasó durante una acción y que el servidor debe avisar a TODOS los clientes
    // (movimiento, pago, carta, cambio de turno, eliminación...).
    public class EventoJuego
    {
        public string Accion { get; }       //constante de Acciones (ej. JUGADOR_MOVIDO)
        public int? JugadorId { get; }
        public string Mensaje { get; }      //texto listo para mostrar en la GUI
        public object? Datos { get; }       //objeto que el servidor serializa en el campo Datos

        public EventoJuego(string accion, int? jugadorId, string mensaje, object? datos)
        {
            Accion = accion;
            JugadorId = jugadorId;
            Mensaje = mensaje;
            Datos = datos;
        }
    }

    // Lo que devuelve cada método público de Juego. El servidor lo traduce:
    //   Exito/Codigo/Mensaje/Datos -> RespuestaMensaje para quien pidió la acción
    //   Eventos                    -> Notificaciones a todos
    //   Dados                      -> MOSTRAR_DADO al hardware
    //   NotificarEstado            -> ESTADO_ACTUALIZADO a todos
    public class ResultadoAccion
    {
        public bool Exito { get; private set; }
        public string Codigo { get; private set; } = "";
        public string Mensaje { get; private set; } = "";
        public object? Datos { get; set; }
        public int? JugadorId { get; set; }
        public DatosDados? Dados { get; set; }
        public bool NotificarEstado { get; set; }

        private ListaDoblementeEnlazada<EventoJuego> eventos = new ListaDoblementeEnlazada<EventoJuego>();

        public IEnumerable<EventoJuego> Eventos
        {
            get { return eventos.RecorrerDesdeInicio(); }
        }

        internal void AsignarEventos(ListaDoblementeEnlazada<EventoJuego> lista)
        {
            eventos = lista;
        }

        public static ResultadoAccion Ok(string mensaje, object? datos = null)
        {
            return new ResultadoAccion { Exito = true, Mensaje = mensaje, Datos = datos, NotificarEstado = true };
        }

        public static ResultadoAccion Error(string codigo, string mensaje)
        {
            return new ResultadoAccion { Exito = false, Codigo = codigo, Mensaje = mensaje };
        }
    }
}