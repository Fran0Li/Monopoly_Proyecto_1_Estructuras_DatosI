namespace MonopolyCore.Estructuras {
class ListaDoblementeEnlazada<T>
{
    private NodoDoble<T>? cabeza;
    private NodoDoble<T>? cola;
    private int cantidad;

    public ListaDoblementeEnlazada()
    {
        this.cabeza = null;
        this.cola = null;
        this.cantidad = 0;
    }

    public int GetCantidad()
    {
        return cantidad;
    }
    public void AgregarAlFinal(T Valor)
    {
        NodoDoble<T> nuevoNodo = new NodoDoble<T>(Valor);
        if (cantidad == 0)
        {
            this.cabeza = this.cola = nuevoNodo;
        }
        else{this.cola = nuevoNodo;}
        this.cantidad++;
    }
    public void RecorrerDesdeInicio()
    {
        
    }
    public void RecorrerDesdeFinal()
    {
        
    }

}
}