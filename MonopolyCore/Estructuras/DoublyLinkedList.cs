namespace MonopolyCore.Estructuras {

public class ListaDoblementeEnlazada<T>
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
        else
        {
            nuevoNodo.Anterior = cola;
            cola!.Siguiente = nuevoNodo;
            this.cola = nuevoNodo;
        }
        this.cantidad++;
    }
    public IEnumerable<T> RecorrerDesdeInicio()
    {
        NodoDoble<T>? actual = cabeza;
        while (actual != null)
            {
                yield return actual.Valor;
                actual = actual.Siguiente;
            }
    }
    public IEnumerable<T> RecorrerDesdeFinal()
    {
        NodoDoble<T>? actual = cola;
        while (actual != null)
            {
                yield return actual.Valor;
                actual = actual.Anterior;
            }
    }
    public IEnumerable<T> BuscarPor(Func<T, bool> predicado)
    {
    NodoDoble<T>? actual = cabeza;

    while (actual != null)
    {
        if (predicado(actual.Valor))
        {
            yield return actual.Valor;
        }

        actual = actual.Siguiente;
    }
    }
}
}