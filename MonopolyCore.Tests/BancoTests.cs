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

        [TestMethod]
        public void PagarMontoCero_NoPermiteTransaccion()
        {
            Banco banco = new Banco();

            Jugador jugador = new Jugador(1, "Esteban");
            jugador.saldo = 1000;

            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => banco.Pagar(jugador,null,0,TipoTransaccion.PagoAlBanco,"Monto inválido",1));

            Assert.AreEqual(1000, jugador.saldo);
            Assert.AreEqual(0, banco.CantidadTransacciones);
        }

        [TestMethod]
        public void PagarSinOrigenNiDestino_NoPermiteTransaccion()
        {
            Banco banco = new Banco();

            Assert.ThrowsExactly<InvalidOperationException>(() => banco.Pagar(null,null,100,TipoTransaccion.PagoAlBanco,"Transacción inválida",1));

            Assert.AreEqual(0, banco.CantidadTransacciones);
        }

        [TestMethod]
        public void PagarVariasVeces_GeneraIdsConsecutivos()
        {
            Banco banco = new Banco();

            Jugador jugador = new Jugador(1, "Esteban");
            jugador.saldo = 1000;

            Transaccion primera = banco.Pagar(jugador,null,100,TipoTransaccion.PagoAlBanco,"Primera",1);

            Transaccion segunda = banco.Pagar(jugador,null,100,TipoTransaccion.PagoAlBanco,"Segunda",2);

            Assert.AreEqual(1, primera.Id);
            Assert.AreEqual(2, segunda.Id);
        }

        [TestMethod]
        public void ConsultarHistorialCompleto_RespetaAmbosSentidos()
        {
            Banco banco = new Banco();

            Jugador jugador = new Jugador(1, "Esteban");
            jugador.saldo = 1000;

            banco.Pagar(jugador,null,100,TipoTransaccion.PagoAlBanco,"Primera",1);
            banco.Pagar(jugador,null,100,TipoTransaccion.PagoAlBanco,"Segunda",2);
            banco.Pagar(jugador,null,100,TipoTransaccion.PagoAlBanco,"Tercera",3);

            int[] desdeInicio = new int[3];
            int indice = 0;

            foreach (Transaccion transaccion in banco.ConsultarHistorialCompleto(true))
            {
                desdeInicio[indice] = transaccion.Id;
                indice++;
            }

            Assert.AreEqual(1, desdeInicio[0]);
            Assert.AreEqual(2, desdeInicio[1]);
            Assert.AreEqual(3, desdeInicio[2]);

            int[] desdeFinal = new int[3];
            indice = 0;

            foreach (Transaccion transaccion in banco.ConsultarHistorialCompleto(false))
            {
                desdeFinal[indice] = transaccion.Id;
                indice++;
            }

            Assert.AreEqual(3, desdeFinal[0]);
            Assert.AreEqual(2, desdeFinal[1]);
            Assert.AreEqual(1, desdeFinal[2]);
        }

        [TestMethod]
        public void ExportarHistorialTxt_CreaArchivoConTransacciones()
        {
            Banco banco = new Banco();

            Jugador jugador = new Jugador(1, "Esteban");
            jugador.saldo = 1000;

            banco.Pagar(jugador,null,200,TipoTransaccion.PagoAlBanco,"Pago de prueba",1);

            string ruta = Path.Combine(Path.GetTempPath(),$"monopoly_{Guid.NewGuid()}.txt");

            try
            {
                banco.ExportarHistorialTxt(ruta);
                Assert.IsTrue(File.Exists(ruta));

                string contenido = File.ReadAllText(ruta);

                StringAssert.Contains(contenido,"Pago de prueba");
                StringAssert.Contains(contenido,"Jugador 1");
                StringAssert.Contains(contenido,"Banco");
            }
            finally
            {
                if (File.Exists(ruta))
                {
                    File.Delete(ruta);
                }
            }
        }
    }
}