using System.Text.Json;
using MonopolyCore.Comunicacion;

namespace MonopolyCore.Tests
{
    [TestClass]
    public class ProtocoloTests
    {
        [TestMethod]
        public void DatosDados_SeSerializanCorrectamente()
        {
            DatosDados dados = new DatosDados
            {
                Valor1 = 4,
                Valor2 = 6
            };

            string json = JsonSerializer.Serialize(dados);

            Assert.Contains("\"Valor1\":4", json);
            Assert.Contains("\"Valor2\":6", json);
        }

        [TestMethod]
        public void DatosDados_SeSerializanYDeserializanCorrectamente()
        {
            DatosDados dados = new DatosDados
            {
                Valor1 = 4,
                Valor2 = 6
            };

            string json = JsonSerializer.Serialize(dados);

            DatosDados? resultado =
                JsonSerializer.Deserialize<DatosDados>(json);

            Assert.IsNotNull(resultado);
            Assert.AreEqual(4, resultado.Valor1);
            Assert.AreEqual(6, resultado.Valor2);
        }


    }


}