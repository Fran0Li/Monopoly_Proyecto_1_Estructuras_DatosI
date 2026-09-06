using MonopolyCore.Estructuras;

namespace MonopolyCore.Estructuras
{
    public class ColaCircular<T>
    {
        private NodoDoble<T>? head;
        private NodoDoble<T>? current;
        private int size;

        public int Size
        {
            get { return size; }
        }

        public ColaCircular()
        {
            head = null;
            current = null;
            size = 0;
        }

        public bool EstaVacia()
        {
            return size == 0;
        }

        public void Encolar(T valor)
        {
            NodoDoble<T> nuevoNodo = new NodoDoble<T>(valor);

            if (head == null)
            {
                head = nuevoNodo;
                current = nuevoNodo;

                nuevoNodo.Siguiente = nuevoNodo;
                nuevoNodo.Anterior = nuevoNodo;
            }
            else
            {
                NodoDoble<T> last = head.Anterior!;

                nuevoNodo.Siguiente = head;
                nuevoNodo.Anterior = last;

                last.Siguiente = nuevoNodo;
                head.Anterior = nuevoNodo;
            }

            size++;

        }

        public T Actual()
        {
            if (current == null)
            {
                throw new InvalidOperationException("La cola está vacía.");
            }
            else
            {
                return current.Valor;
            }
        }

        public T AvanzarTurno()
        {
            if (current == null)
            {
                throw new InvalidOperationException("La cola está vacía.");
            }
            else
            {
                current = current.Siguiente!;
                return current.Valor;
            }
        }

        public void EliminarActual()
        {
            if (current == null)
            {
                return;
            }
            if (size == 1)
            {
                head = null;
                current = null;
                size = 0;
                return;
            }

            NodoDoble<T> previus = current.Anterior!;
            NodoDoble<T> next = current.Siguiente!;

            previus.Siguiente = next;
            next.Anterior = previus;

            if (current == head)
            {
                head = next;
            }

            current = next;

            size--;
        }
    }
}
