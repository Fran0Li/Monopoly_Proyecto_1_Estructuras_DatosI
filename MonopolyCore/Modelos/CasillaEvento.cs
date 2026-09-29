using MonopolyCore.Estructuras;
namespace MonopolyCore.Modelos{
public class CasillaEvento : Casilla
{
    //constructor de la clase casilla evento
    public CasillaEvento(int id, string nombre, int posicion)
        : base(id, nombre, posicion, TipoCasilla.Evento)
    {
        
    }
    //metodo para tomar una carta del mazo de cartas de evento
    //la carta tomada pasa al final del mazo para reutilizarse después
    public CartaEvento TomarCarta(ListaCircularDoble<CartaEvento> mazo)
    {
        return mazo.TomarPrimeroYEnviarAlFinal();
    }

    public override void AlCaer(Jugador jugador, Juego juego)
    {
        CartaEvento carta = juego.TomarCarta(jugador);
        carta.Aplicar(jugador, juego);
    }
}
}