namespace MonopolyCore.Modelos {
public class CasillaEspecial : Casilla
{
    private TipoCasillaEspecial tipoespecial;

    public TipoCasillaEspecial TipoEspecial
    {
        get { return tipoespecial; }
        set { tipoespecial = value; }
    }
    private int monto; //solo se usa en Impuesto

    public int Monto
    {
        get { return monto; }
        set { monto = value; }
    }
    public CasillaEspecial(int id, string nombre, int posicion, TipoCasillaEspecial tipoespecial, int monto = 0)
        : base(id, nombre, posicion, TipoCasilla.Especial)
    {
        this.tipoespecial = tipoespecial;
        this.monto = monto;
    }

    public override void AlCaer(Jugador jugador, Juego juego)
    {
        switch (tipoespecial)
        {
            case TipoCasillaEspecial.IrACarcel:
                juego.EnviarACarcel(jugador);
                break;
            case TipoCasillaEspecial.Impuesto:
                juego.CobrarObligatorio(jugador, null, monto, TipoTransaccion.PagoAlBanco, $"Impuesto: {Nombre}");
                break;
            case TipoCasillaEspecial.Carcel:
                juego.Informar(jugador, $"{jugador.nombre} está de visita en la cárcel.");
                break;
            default: //Inicio (el premio se cobra al pasar) y ParqueoGratis no hacen nada
                juego.Informar(jugador, $"{jugador.nombre} cayó en {Nombre}.");
                break;
        }
    }
}
}