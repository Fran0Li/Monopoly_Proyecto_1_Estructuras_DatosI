using MonopolyCore.Estructuras;

namespace MonopolyCore.Tests
{
    [TestClass]
    public class NodoDobleTests
    {
        [TestMethod]
        public void Constructor_InicializaCorrectamente()
        {
            NodoDoble<string> nodo = new NodoDoble<string>("Esteban");

            Assert.AreEqual("Esteban", nodo.Valor);
            Assert.IsNull(nodo.Siguiente);
            Assert.IsNull(nodo.Anterior);
        }

        [TestMethod]
        public void Nodos_PuedenConectarseEnAmbasDirecciones()
        {
            NodoDoble<string> nodo1 = new NodoDoble<string>("Esteban");
            NodoDoble<string> nodo2 = new NodoDoble<string>("Fran");

            nodo1.Siguiente = nodo2;
            nodo2.Anterior = nodo1;

            Assert.AreSame(nodo2, nodo1.Siguiente);
            Assert.AreSame(nodo1, nodo2.Anterior);
        }
    }
}