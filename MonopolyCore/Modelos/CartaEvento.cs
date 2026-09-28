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
    private decimal Valor
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

}
}