namespace MonopolyCore.Modelos {
public abstract class Casilla
{
    private int id; //identificador unico de la casilla
    private string nombre;//nombre de la casilla
    private int posicion;//posicion de la casilla en el tablero
    private TipoCasilla tipo;//si la casilla es propiedad, evento o especial

//setters y getters de las propiedades de la casilla
    public int Id 
    {
        get { return id; }
        set { id = value;}
        
    }
    public string Nombre
    {
        get { return nombre; }
        set { nombre = value; }
    }

    public int Posicion
    {
        get { return posicion; }
        set { posicion = value; }
    }
    public TipoCasilla Tipo
    {
        get { return tipo; }
        protected set { tipo = value; }
    }
    //constructor de la clase casilla, recibe el id, nombre, posicion y tipo de la casilla
    protected Casilla(int id, string nombre, int posicion, TipoCasilla tipo)
    {
        this.id = id;
        this.nombre = nombre;
        this.posicion = posicion;
        this.tipo = tipo;
    }

}
}
