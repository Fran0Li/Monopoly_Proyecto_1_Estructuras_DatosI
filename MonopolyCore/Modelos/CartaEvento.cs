namespace MonopolyCore.Modelos {
public class CartaEvento
{
    private int id;
    private string descripcion;
    private TipoEfectoEvento tipoEfecto;
    private decimal valor;

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
    private decimal Valor
    {
        get { return valor; }
        set { valor = value; }
    }

    public CartaEvento(int id, string descripcion, TipoEfectoEvento tipoEfecto, decimal valor)
    {
        this.id = id;
        this.descripcion = descripcion;
        this.tipoEfecto = tipoEfecto;
        this.valor = valor;
    }

}
}