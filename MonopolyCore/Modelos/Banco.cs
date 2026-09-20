using MonopolyCore.Estructuras;
using MonopolyCore.Modelos;

namespace MonopolyCore
{
    public class Banco
    {
        // El historial utiliza la lista doblemente enlazada definida por el proyecto.
        // Cada pago válido genera automáticamente una nueva transacción.
        private readonly ListaDoblementeEnlazada<Transaccion> historial;

        // Permite asignar un identificador único y consecutivo a cada transacción.
        private int siguienteTransaccionId;

        // Constructor
        public Banco()
        {
            historial = new ListaDoblementeEnlazada<Transaccion>();
            siguienteTransaccionId = 1;
        }

        // Permite consultar cuántas transacciones se han registrado.
        public int CantidadTransacciones
        {
            get { return historial.GetCantidad(); }
        }

        // Comprueba si un jugador posee suficiente saldo para realizar un pago.
        public bool TieneSaldoSuficiente(Jugador jugador, int monto)
        {
            if (jugador == null) //No se hace si no hay un jugador
            {
                throw new ArgumentNullException(nameof(jugador));
            }

            if (monto <= 0) //Tampoco si no hay dinero
            {
                return false;
            }

            return jugador.saldo >= monto;
        }

        // Un origen null representa al Banco pagando.
        // Un destino null representa al Banco recibiendo.
        public Transaccion Pagar(Jugador? origen,Jugador? destino,int monto,TipoTransaccion tipo,string descripcion,int numeroTurno)
        {
            // Todas las validaciones se realizan antes de modificar los saldos
            // para evitar dejar el estado del juego inconsistente.

            if (monto <= 0) //Si no se envia un monto valido
            {
                throw new ArgumentOutOfRangeException(nameof(monto),"El monto debe ser mayor que cero.");
            }

            // Una transacción siempre debe involucrar al menos
            // a un jugador o al Banco.
            if (origen == null && destino == null)
            {
                throw new InvalidOperationException("La transacción debe tener un origen o un destino.");
            }

            // Si el origen es un jugador, debe poseer suficiente saldo.
            if (origen != null && !TieneSaldoSuficiente(origen, monto))
            {
                throw new InvalidOperationException("Saldo insuficiente.");
            }

            // Un jugador no puede pasarse dinero a el mismo.
            if (origen != null && ReferenceEquals(origen, destino))
            {
                throw new InvalidOperationException("El origen y el destino no pueden ser el mismo jugador.");
            }

            // Se obtienen los identificadores que se almacenarán
            // en el historial de transacciones.
            int? origenId = origen?.ObtenerId();
            int? destinoId = destino?.ObtenerId();

            // Si el origen es un jugador se descuenta el dinero.
            // Cuando origen es null, significa que paga el Banco.

            if (origen != null)
            {
                origen.saldo -= monto;
            }

            // Si el destino es un jugador se acredita el dinero.
            // Cuando destino es null, significa que recibe el Banco.
            if (destino != null)
            {
                destino.saldo += monto;
            }

            // Se registra la operación después de modificar correctamente
            // los saldos involucrados.
            Transaccion transaccion = new Transaccion(siguienteTransaccionId,numeroTurno,tipo,origenId,destinoId,monto,descripcion);
            historial.AgregarAlFinal(transaccion);
            siguienteTransaccionId++;

            return transaccion;
        }

        // Busca todas las transacciones en las que participó un jugador,
        // ya sea como origen o como destino.
        public IEnumerable<Transaccion> ConsultarHistorialPorJugador(int jugadorId)
        {
            return historial.BuscarPor(transaccion => transaccion.JugadorOrigenId == jugadorId || transaccion.JugadorDestinoId == jugadorId);
        }

        // Busca únicamente las transacciones que coincidan
        // con el tipo solicitado.
        public IEnumerable<Transaccion> ConsultarHistorialPorTipo(TipoTransaccion tipo)
        {
            return historial.BuscarPor(transaccion => transaccion.Tipo == tipo);
        }

        // Devuelve todo el historial en el sentido solicitado.
        // true  -> de la transacción más antigua a la más reciente.
        // false -> de la transacción más reciente a la más antigua.
        public IEnumerable<Transaccion> ConsultarHistorialCompleto(bool desdeInicio)
        {
            if (desdeInicio)
            {
                return historial.RecorrerDesdeInicio();
            }

            return historial.RecorrerDesdeFinal();
        }
    }
}