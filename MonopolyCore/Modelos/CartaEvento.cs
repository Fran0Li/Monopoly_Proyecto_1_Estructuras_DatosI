namespace MonopolyCore.Modelos {
public class CartaEvento
{
    private int id; // Identificador único de la carta de evento
    private string descripcion; // Descripción del efecto de la carta de evento
    private TipoEfectoEvento tipoEfecto;// Tipo de efecto que tiene la carta de evento (por ejemplo, pagar, recibir, avanzar, retroceder, etc.)
    private decimal valor;

//getters y setters
    public int Id
    {
        get { return id; }
        set { id = value; }
    }

    public string Descripcion
    {
        get { return descripcion; }
        set { descripcion = value; }
    }
    public TipoEfectoEvento TipoEfecto
    {
        get { return tipoEfecto; }
        set { tipoEfecto = value; }
    }
    public decimal Valor
    {
        get { return valor; }
        set { valor = value; }
    }
    //constructor

    public CartaEvento(int id, string descripcion, TipoEfectoEvento tipoEfecto, decimal valor)
    {
        this.id = id;
        this.descripcion = descripcion;
        this.tipoEfecto = tipoEfecto;
        this.valor = valor;
    }

    //Aplica el efecto de la carta. Valor = monto, cantidad de pasos, casilla destino o turnos a perder.
    public void Aplicar(Jugador jugador, Juego juego)
    {
        int cantidad = (int)valor;
        switch (tipoEfecto)
        {
            case TipoEfectoEvento.GanarDinero:
                juego.PagarDesdeBanco(jugador, cantidad, TipoTransaccion.GananciaEvento, descripcion);
                break;
            case TipoEfectoEvento.PerderDinero:
                juego.CobrarObligatorio(jugador, null, cantidad, TipoTransaccion.PerdidaEvento, descripcion);
                break;
            case TipoEfectoEvento.Moverse: //avanzar
                juego.MoverJugador(jugador, cantidad);
                break;
            case TipoEfectoEvento.Retroceder:
                juego.MoverJugador(jugador, -cantidad);
                break;
            case TipoEfectoEvento.IrACasilla:
                juego.MoverJugadorA(jugador, cantidad);
                break;
            case TipoEfectoEvento.PerderTurno:
                juego.HacerPerderTurnos(jugador, cantidad > 0 ? cantidad : 1);
                break;
            case TipoEfectoEvento.IrACarcel:
                juego.EnviarACarcel(jugador);
                break;
            case TipoEfectoEvento.SalirDeCarcelGratis:
                jugador.turnosEnCarcel = 0;
                juego.Informar(jugador, $"{jugador.nombre} ya no pierde turnos.");
                break;
        }
    }

}
}