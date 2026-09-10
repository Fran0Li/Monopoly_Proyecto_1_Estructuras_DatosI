using MonopolyCore.Modelos;

namespace MonopolyCore.Tests
{
    [TestClass]
    public class DadoTests
    {
        [TestMethod]
        public void Lanzar_GeneraValoresEntreUnoYSeis()
        {
            Dado dado = new Dado();

            for (int i = 0; i < 100; i++)
            {
                dado.Lanzar();

                Assert.IsTrue(dado.Valor1 >= 1 && dado.Valor1 <= 6);
                Assert.IsTrue(dado.Valor2 >= 1 && dado.Valor2 <= 6);
            }
        }
    }
}