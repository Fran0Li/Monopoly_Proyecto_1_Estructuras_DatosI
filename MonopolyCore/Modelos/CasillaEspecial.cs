public class CasillaEspecial : Casilla
{
    private TipoCasillaEspecial tipoespecial;

    public TipoCasillaEspecial TipoEspecial
    {
        get { return tipoespecial; }
        set { tipoespecial = value; }
    }
    public CasillaEspecial(int id, string nombre, int posicion, TipoCasillaEspecial tipoespecial)
        : base(id, nombre, posicion, TipoCasilla.Especial)
    {
        this.tipoespecial = tipoespecial;
    }
}