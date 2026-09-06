namespace MonopolyCore.Estructuras
{
    public class NodoDoble<T>
    {
        public T Valor { get; set; }

        public NodoDoble<T>? Siguiente { get; set; }

        public NodoDoble<T>? Anterior { get; set; }

        public NodoDoble(T valor)
        {
            Valor = valor;
            Siguiente = null;
            Anterior = null;
        }
    }
}