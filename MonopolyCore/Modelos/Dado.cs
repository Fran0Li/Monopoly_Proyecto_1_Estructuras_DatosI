namespace MonopolyCore.Modelos
{
    public class Dado
    {
        //Devielve dos valores ademas estos solo pueden cambiar con tirar dados
        public int Valor1 { get; private set; }
        public int Valor2 { get; private set; }

        //Escoge dos valores al azar dentro del rango de 1 a 6, el 7 se ignora por la operacion Ramdom
        public (int, int) Lanzar()
        {
            Valor1 = Random.Shared.Next(1, 7);
            Valor2 = Random.Shared.Next(1, 7);

            return (Valor1, Valor2);
        }

        //Devuelve el valor total de la suma de los dados
        public int Total()
        {
            return Valor1 + Valor2;
        }

        //Verifica si los dados salieron dobles, es decir si son igulaes
        public bool EsDoble()
        {
            return Valor1 == Valor2;
        }
    }
}