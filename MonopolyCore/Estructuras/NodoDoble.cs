public class NodoDoble<T>
{
    public T Valor { get; set;} //obtiene y establece el valor del nodo
    public NodoDoble<T>? Siguiente { get; set; } //obtiene y establece el nodo siguiente al nodo actual
    public NodoDoble<T>? Anterior { get; set; } //obtiene y establece el nodo anterior al nodo actual

    public NodoDoble(T valor)   //constructor del nodo que recibe un valor y lo asigna a la propiedad Valor
    {
        Valor = valor; //asigna el valor proporcionado a la propiedad Valor
    }
}