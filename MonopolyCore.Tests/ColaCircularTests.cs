using MonopolyCore.Estructuras;

namespace MonopolyCore.Tests
{
    [TestClass]
    public class ColaCircularTests
    {
        [TestMethod]
        public void ColaNueva_EstaVacia()
        {
            ColaCircular<string> cola = new ColaCircular<string>();

            Assert.IsTrue(cola.EstaVacia());
            Assert.AreEqual(0, cola.Size);
        }

        [TestMethod]
        public void Encolar_AumentaCantidad()
        {
            ColaCircular<string> cola = new ColaCircular<string>();

            cola.Encolar("Esteban");

            Assert.IsFalse(cola.EstaVacia());
            Assert.AreEqual(1, cola.Size);

            cola.Encolar("Fran");

            Assert.AreEqual(2, cola.Size);
        }

        [TestMethod]
        public void Actual_DevuelvePrimerElemento()
        {
            ColaCircular<string> cola = new ColaCircular<string>();

            cola.Encolar("Esteban");
            cola.Encolar("Fran");
            cola.Encolar("Julian");
            cola.Encolar("Andrew");

            Assert.AreEqual("Esteban", cola.Actual());
        }

        [TestMethod]
        public void AvanzarTurno_RecorreCircularmente()
        {
            ColaCircular<string> cola = new ColaCircular<string>();

            cola.Encolar("Esteban");
            cola.Encolar("Fran");
            cola.Encolar("Julian");
            cola.Encolar("Andrew");

            Assert.AreEqual("Esteban", cola.Actual());

            Assert.AreEqual("Fran", cola.AvanzarTurno());
            Assert.AreEqual("Julian", cola.AvanzarTurno());
            Assert.AreEqual("Andrew", cola.AvanzarTurno());

            Assert.AreEqual("Esteban", cola.AvanzarTurno());
        }

        [TestMethod]
        public void EliminarActual_EliminaYAvanzaAlSiguiente()
        {
            ColaCircular<string> cola = new ColaCircular<string>();

            cola.Encolar("Esteban");
            cola.Encolar("Fran");
            cola.Encolar("Julian");
            cola.Encolar("Andrew");

            cola.AvanzarTurno();

            Assert.AreEqual("Fran", cola.Actual());

            cola.EliminarActual();

            Assert.AreEqual(3, cola.Size);
            Assert.AreEqual("Julian", cola.Actual());
        }

        [TestMethod]
        public void EliminarActual_SiEsHead_ActualizaHead()
        {
            ColaCircular<string> cola = new ColaCircular<string>();

            cola.Encolar("Esteban");
            cola.Encolar("Fran");
            cola.Encolar("Julian");

            cola.EliminarActual();

            Assert.AreEqual(2, cola.Size);
            Assert.AreEqual("Fran", cola.Actual());

            cola.AvanzarTurno();

            Assert.AreEqual("Julian", cola.Actual());

            cola.AvanzarTurno();

            Assert.AreEqual("Fran", cola.Actual());
        }

        [TestMethod]
        public void EliminarActual_UnicoElemento_DejaColaVacia()
        {
            ColaCircular<string> cola = new ColaCircular<string>();

            cola.Encolar("Esteban");

            cola.EliminarActual();

            Assert.IsTrue(cola.EstaVacia());
            Assert.AreEqual(0, cola.Size);
        }
    }
}