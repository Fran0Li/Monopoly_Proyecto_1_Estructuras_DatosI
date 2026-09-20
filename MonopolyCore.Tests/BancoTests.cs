using MonopolyCore.Modelos;

namespace MonopolyCore.Tests
{
    [TestClass]
    public class BancoTests
    {
        [TestMethod]
        public void PagarJugadorAlBanco_RestaSaldo()
        {
            Banco banco = new Banco();
            Jugador jugador = new Jugador(1, "Esteban");
            jugador.Saldo = 1000;

            Transaccion transaccion = banco.Pagar(
                jugador,
                null,
                250,
                TipoTransaccion.PagoAlBanco,
                "Prueba de pago",
                1);

            Assert.AreEqual(750, jugador.Saldo);
            Assert.AreEqual(1,transaccion.JugadorOrigenId);
            Assert.IsNull(transaccion.JugadorDestinoId);
            Assert.AreEqual(1,banco.CantidadTransacciones);
        }

        [TestMethod]
        public void PagarBancoAJugador_AumentaSaldo()
        {
            Banco banco = new Banco();
            Jugador jugador = new Jugador(1, "Esteban");
            jugador.Saldo = 500;

            banco.Pagar(
                null,
                jugador,
                200,
                TipoTransaccion.GananciaEvento,
                "Premio",
                1);

            Assert.AreEqual(700,jugador.Saldo);
        }

        [TestMethod]
        public void PagarEntreJugadores_MueveDinero()
        {
            Banco banco = new Banco();
            Jugador origen = new Jugador(1, "Esteban");
            Jugador destino = new Jugador(2, "Fran");
            origen.Saldo = 1000;
            destino.Saldo = 500;

            banco.Pagar(
                origen,
                destino,
                300,
                TipoTransaccion.PagoAlquiler,
                "Pago de alquiler",
                2);

            Assert.AreEqual(700,origen.Saldo);
            Assert.AreEqual(800,destino.Saldo);
        }

        [TestMethod]
        public void PagarSinSaldo_NoModificaNada()
        {
            Banco banco = new Banco();
            Jugador jugador = new Jugador(1, "Esteban");
            jugador.Saldo = 100;

            Assert.ThrowsExactly<InvalidOperationException>(
                () => banco.Pagar(
                    jugador,
                    null,
                    500,
                    TipoTransaccion.PagoAlBanco,
                    "Pago imposible",
                    1
                )
            );

            Assert.AreEqual(100,jugador.Saldo);
            Assert.AreEqual(0,banco.CantidadTransacciones);
        }
    }
}