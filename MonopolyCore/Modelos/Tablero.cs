using MonopolyCore.Estructuras;
namespace MonopolyCore.Modelos {
public class Tablero
{
    private ListaCircularDoble<Casilla> casillas; //lista circular doble de casillas que representa el tablero
    public int CantidadCasillas //metodo que devuelve la cantidad de casillas en el tablero
    {
        get { return casillas.Cantidad; }
    }
    public Tablero() //constructor del tablero
    {
        casillas = new ListaCircularDoble<Casilla>();

    }
    public void AgregarCasilla(Casilla casilla) //metodo para agregar una casilla al final de la lista (NO ESTABA EN EL UML, VER ESO)
    {
        casillas.AgregarAlFinal(casilla);
    }
    public Casilla ObtenerCasilla(int posicion) //metodo para obtener en que casilla se encuentra un jugador
    {
        return casillas.ObtenerEnPosicion(posicion);
    }
    public int CalcularNuevaPosicion(int posicionActual, int pasos) //metodo para calcular la posicion nueva al avanzar una casilla
    {
        int total = casillas.Cantidad;
        int nuevaPosicion = ((posicionActual + pasos) % total + total)% total;
        return nuevaPosicion;
    }
    public bool PasoPorInicio(int posicionAnterior, int posicionNueva) //metodo que retorna true o false dependiendo de si el jugador pasó por el inicio o no
    {
        return posicionNueva < posicionAnterior;
    }
}
}