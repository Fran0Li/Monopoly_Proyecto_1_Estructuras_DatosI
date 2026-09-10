namespace MonopolyCore.Estructuras //Espacio de nombres donde se agrupan las estructuras del proyecto
{
    public class NodoDoble<T>   //Clase genérica ("T") que representa un nodo doblemente enlazado
    {
        public T Valor { get; set; } //Dato a ingresar en el nodo

        public NodoDoble<T>? Siguiente { get; set; } //Referencia al siguiente nodo

        public NodoDoble<T>? Anterior { get; set; } //Referencia al nodo anterior

        public NodoDoble(T valor)  //Constructor que inicializa el nodo con un valor
        {
            Valor = valor;
            //Al crearse, el nodo todavía no está conectado a otros nodos
            Siguiente = null;
            Anterior = null;
        }
    }
}