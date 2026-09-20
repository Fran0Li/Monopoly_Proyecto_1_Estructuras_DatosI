using MonopolyCore;
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
            jugador.saldo = 1000;

            Transaccion transaccion = banco.Pagar(jugador,null,250,TipoTransaccion.PagoAlBanco,"Prueba de pago",1);

            Assert.AreEqual(750, jugador.saldo);
            Assert.AreEqual(1, transaccion.JugadorOrigenId);
            Assert.IsNull(transaccion.JugadorDestinoId);
            Assert.AreEqual(1, banco.CantidadTransacciones);
        }

        [TestMethod]
        public void PagarBancoAJugador_AumentaSaldo()
        {
            Banco banco = new Banco();
            Jugador jugador = new Jugador(1, "Esteban");
            jugador.saldo = 500;

            banco.Pagar(null,jugador,200,TipoTransaccion.GananciaEvento,"Premio",1);

            Assert.AreEqual(700, jugador.saldo);
        }

        [TestMethod]
        public void PagarEntreJugadores_MueveDinero()
        {
            Banco banco = new Banco();

            Jugador origen = new Jugador(1, "Esteban");
            Jugador destino = new Jugador(2, "Fran");

            origen.saldo = 1000;
            destino.saldo = 500;

            banco.Pagar(origen,destino,300,TipoTransaccion.PagoAlquiler,"Pago de alquiler",2);

            Assert.AreEqual(700, origen.saldo);
            Assert.AreEqual(800, destino.saldo);
        }

        [TestMethod]
        public void PagarSinSaldo_NoModificaNada()
        {
            Banco banco = new Banco();

            Jugador jugador = new Jugador(1, "Esteban");
            jugador.saldo = 100;

            Assert.ThrowsExactly<InvalidOperationException>(() => banco.Pagar(jugador,null, 500,TipoTransaccion.PagoAlBanco, "Pago imposible",1));

            Assert.AreEqual(100, jugador.saldo);
            Assert.AreEqual(0, banco.CantidadTransacciones);
        }

        [TestMethod]
        public void PagarAlMismoJugador_NoPermiteTransaccion()
        {
            Banco banco = new Banco();

            Jugador jugador = new Jugador(1, "Esteban");
            jugador.saldo = 1000;

            Assert.ThrowsExactly<InvalidOperationException>(
                () => banco.Pagar(
                    jugador,
                    jugador,
                    100,
                    TipoTransaccion.PagoEntreJugadores,
                    "Pago inválido",
                    1
                )
            );

            Assert.AreEqual(1000, jugador.saldo);
            Assert.AreEqual(0, banco.CantidadTransacciones);
        }

        [TestMethod]
        public void ConsultarHistorialPorJugador_EncuentraSusTransacciones()
        {
            Banco banco = new Banco();

            Jugador jugador1 = new Jugador(1, "Esteban");
            Jugador jugador2 = new Jugador(2, "Fran");

            jugador1.saldo = 1000;
            jugador2.saldo = 1000;

            banco.Pagar(jugador1,null,100,TipoTransaccion.PagoAlBanco,"Pago del jugador 1",1);

            banco.Pagar(jugador2,null,200,TipoTransaccion.PagoAlBanco,"Pago del jugador 2",2);

            int cantidad = 0;

            foreach (Transaccion transaccion in banco.ConsultarHistorialPorJugador(1))
            {
                cantidad++;
            }

            Assert.AreEqual(1, cantidad);
        }

        [TestMethod]
        public void ConsultarHistorialPorTipo_EncuentraTipoCorrecto()
        {
            Banco banco = new Banco();

            Jugador jugador = new Jugador(1, "Esteban");
            jugador.saldo = 1000;

            banco.Pagar(jugador,null,100,TipoTransaccion.PagoAlBanco,"Pago",1);

            banco.Pagar(null,jugador,200,TipoTransaccion.GananciaEvento,"Premio",2);

            int cantidad = 0;

            foreach (Transaccion transaccion in banco.ConsultarHistorialPorTipo(TipoTransaccion.GananciaEvento))
            {
                cantidad++;
            }

            Assert.AreEqual(1, cantidad);
        }
    }
}