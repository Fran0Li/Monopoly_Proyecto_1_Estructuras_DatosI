namespace MonopolyCore
{
    public class Banco
    {
        private ListaDoblementeEnlazada<Transaccion> historial;

        public Banco()
        {
            // Inicializar historial
        }

        public Transaccion Pagar(
            Jugador? origen,
            Jugador? destino,
            decimal monto,
            TipoTransaccion tipo,
            string descripcion,
            int numeroTurno)
        {
            // Validar monto
            // Validar saldo del origen
            // Restar al origen
            // Sumar al destino
            // Crear transacción
            // Guardarla en historial
            // Retornarla
        }
    }
}