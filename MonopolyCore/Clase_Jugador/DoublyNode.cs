class NodoDoble<T>
{
    public T Valor;
    public NodoDoble<T>? Siguiente;
    public NodoDoble<T>? Anterior;

    public NodoDoble(T valor)
    {
    this.Valor = valor;
    this.Siguiente = null;
    this.Anterior = null;
    }


}