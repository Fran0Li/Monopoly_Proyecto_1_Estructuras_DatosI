using System;
using System.Collections.Generic;

namespace MonopolyCore.Estructuras {
public class ListaCircularDoble<T>
{
    private NodoDoble<T>? cabeza; //cabeza de la lista
    private int cantidad; //cantidad de elementos en la lista

    public int Cantidad => cantidad; //propiedad para obtener la cantidad de elementos en la lista, devuelve el valor de cantidad
    public void AgregarAlFinal(T valor) //metodo que agrega un elemento al final de la lista
    {
        var nuevo = new NodoDoble<T>(valor); //crea un nuevo nodo con el valor proporcionado

        if (cabeza == null) //si no hay cabeza, significa que la lista está vacía, entonces el nuevo nodo se convierte en la cabeza y apunta a sí mismo como siguiente y anterior
        {
            cabeza = nuevo; //hace que el nuevo nodo sea la cabeza de la lista
            nuevo.Siguiente = nuevo; //hace que el nuevo nodo apunte a sí mismo como siguiente
            nuevo.Anterior = nuevo; //el nuevo noco apunta a si mismo como anterior

        }
        else //si no
        {
            var ultimo = cabeza.Anterior!; //obtiene el ultimo nodo agarrando el anterior de la cabeza
            ultimo.Siguiente = nuevo; //hace que el ultimo nodo apunte al nuevo nodo como siguiente 
            nuevo.Anterior = ultimo; //hace que el nuevo nodo apunte al ultimo nodo como anterior
            nuevo.Siguiente = cabeza; //indica que el nuevo nodo apuntará a la cabeza como siguiente
            cabeza.Anterior = nuevo; // hace que la cabeza apunte al nuevo nodo como anterior
        }
        cantidad++; //incrementa en 1 la cantidad de elementos de la lista
    }
    public NodoDoble<T> ObtenerNodoEnPosicion(int indice)
    {
        if (cabeza == null) //si no hay cabeza, significa que la lista está vacía, entonces lanza una excepción
        {
            throw new InvalidOperationException("La lista está vacía."); //lanza una excepcion indicando que la lista está vacía
        }

        int pos = ((indice%cantidad) + cantidad) % cantidad; //calcula la posicion real del nodo a obtener
            var actual = cabeza; //inicia desde la cabeza de la lista
            for (int i = 0; i < pos; i++) //recorre la lista hasta llegar a la posicion deseada
            actual = actual.Siguiente!; //avanza al siguiente nodo

            return actual; //devuelve el nodo en la posicion deseada

    }
    public T ObtenerEnPosicion(int indice) 
    {
        return ObtenerNodoEnPosicion(indice).Valor; //devuelve el valor del nodo en la posicion deseada
    }

    public NodoDoble<T> Siguiente(NodoDoble<T> nodoActual)
    {
        return nodoActual.Siguiente!; //devuelve el nodo siguiente al nodo actual
    
    }
    public NodoDoble<T> Anterior(NodoDoble<T> nodoActual)
    {
        return nodoActual.Anterior!;//devuelve el nodo que esta antes del actual
    }
    public IEnumerable<T> Recorrer()
    {
        if (cabeza == null)yield break;  //si no hay cabeza, termina la ejecucion

        var actual = cabeza; // inicia desde la cabeza de la lista
        do
        {
            yield return actual.Valor; //devuelve el valor del nodo actual
            actual = actual.Siguiente!; //avanza al nodo que sigue

        }   while (actual != cabeza); //siempre y cuando el nodo actual no sea la cabeza, si si es la cabeza
    }


}
}