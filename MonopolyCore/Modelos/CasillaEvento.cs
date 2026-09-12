public class CasillaEvento : Casilla
{
    //constructor de la clase casilla evento
    public CasillaEvento(int id, string nombre, int posicion)
        : base(id, nombre, posicion, TipoCasilla.Evento)
    {
        
    }
    //metodo para tomar una carta del mazo de cartas de evento
    public CartaEvento TomarCarta(ListaCircularDoble<CartaEvento> mazo)
    {
        CartaEvento carta = mazo.ObtenerEnPosicion(0);
        return carta;
    }
}