namespace MonopolyCore.Estructuras
{
    public class ColaCircular<T>
    {
        private NodoDoble<T>? head; //Cabeza de la cola 
        private NodoDoble<T>? current; //Nodo que representa el turno actual 
        private int size; //Cantidad de elementos en la cola

        public int Size  //Permite consultar el tamaño de la cola
        {
            get { return size; }
        }

        public ColaCircular() //Constructor de una cola vacía
        {
            head = null;
            current = null;
            size = 0;
        }

        public bool EstaVacia() //Verifica si la cola está vacía
        {
            return size == 0;
        }

        public void Encolar(T valor) //Agrega un nuevo elemento al final de la cola
        {
            NodoDoble<T> nuevoNodo = new NodoDoble<T>(valor);

            if (head == null) //Si no existe head, la cola está vacía y el nuevo nodo será el primero
            {
                head = nuevoNodo;
                current = nuevoNodo; //El primer elemento encolado inicia como turno actual

                //El único nodo se apunta a sí mismo para mantener la circularidad
                nuevoNodo.Siguiente = nuevoNodo;
                nuevoNodo.Anterior = nuevoNodo;
            }
            else
            {
                //En una cola circular doblemente enlazada,
                //el nodo anterior a head siempre es el último nodo
                NodoDoble<T> last = head.Anterior!;

                nuevoNodo.Siguiente = head;
                nuevoNodo.Anterior = last;

                last.Siguiente = nuevoNodo;
                head.Anterior = nuevoNodo;
            }

            size++;

        }

        public T Actual() //Consulta el elemento que posee el turno actual
        {
            if (current == null)
            {
                //No existe un turno actual si la cola está vacía
                throw new InvalidOperationException("La cola está vacía.");
            }

            return current.Valor;
        }

        public T AvanzarTurno() //Avanza el turno al siguiente elemento de la cola
        {
            if (current == null)
            {
                //No se puede avanzar el turno si la cola está vacía
                throw new InvalidOperationException("La cola está vacía."); 
            }
            
            current = current.Siguiente!;
            return current.Valor;
        }

        public void EliminarActual() //Elimina de la cola al elemento que posee el turno actual
        {
            if (current == null) //No ocurre nada si la cola está vacía
            {
                return;
            }
            if (size == 1) //Si solo queda un elemento, al eliminarlo la cola queda vacía
            {
                head = null;
                current = null;
                size = 0;
                return;
            }

            //Guardamos los nodos vecinos del jugador actual antes de desconectar al nodo actual
            NodoDoble<T> previous = current.Anterior!;
            NodoDoble<T> next = current.Siguiente!;

            //Conectamos directamente el nodo anterior con el siguiente,
            //eliminando al nodo actual sin romper la circularidad
            previous.Siguiente = next;
            next.Anterior = previous;

            if (current == head) //Si eliminamos head, el siguiente nodo se convierte en la nueva cabeza
            {
                head = next;
            }

            //El turno pasa al siguiente jugador después de eliminar al actual
            current = next;

            size--;
        }
    }
}
