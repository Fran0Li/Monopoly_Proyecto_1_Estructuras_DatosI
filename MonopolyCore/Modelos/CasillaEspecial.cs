namespace MonopolyCore.Modelos {
public class CasillaEspecial : Casilla //hereda de la clase Casilla
{
    private TipoCasillaEspecial tipoespecial; //tipo de casilla especial
    public TipoCasillaEspecial TipoEspecial // getter y setter del tipo de casilla especial
    {
        get { return tipoespecial; }
        set { tipoespecial = value; }
    }
    //constructor de la clase CasillaEspecial, recibe el id, nombre, posicion y tipo de la casilla especial
    public CasillaEspecial(int id, string nombre, int posicion, TipoCasillaEspecial tipoespecial)
        : base(id, nombre, posicion, TipoCasilla.Especial)
    {
        this.tipoespecial = tipoespecial;
    }
}
}