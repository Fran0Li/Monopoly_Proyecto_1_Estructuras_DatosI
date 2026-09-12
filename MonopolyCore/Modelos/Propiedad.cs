public class Propiedad : Casilla
{
    private int precio; //precio de la propiedad
    private decimal alquiler; //precio del alquiler de la propiedad
    private Jugador? propietario;//propietario de la propiedad, puede ser null si no tiene propietario
    private bool hipotecada;//indica si la propiedad esta hipotecada o no
//constructores, getters y setters de la clase propiedad
    public int Precio
    {
        get { return precio; }
        set { precio = value; }
    }
    public decimal Alquiler
    {
        get { return alquiler; }
        set { alquiler = value; }
    }
    public Jugador? Propietario
    {
        get { return propietario; }
        set { propietario = value; }
    }
    public bool Hipotecada
    {
        get { return hipotecada; }
        set { hipotecada = value; }
    }
    //constructor de la clase propiedad
    public Propiedad(int id, string nombre, int posicion, int precio, decimal alquiler)
    : base(id, nombre, posicion, TipoCasilla.Propiedad)
    {
        this.precio = precio;
        this.alquiler = alquiler;
        this.propietario = null;
        this.hipotecada = false;
    }
//metodo para asignar un propietario a la propiedad
    public void AsignarPropietario(Jugador jugador)
    {
        propietario = jugador;
    }
//metodo para hipotecar la propiedad
    public decimal CalcularAlquiler()
    {
        if (hipotecada)
        {
            return 0; // Si la propiedad está hipotecada, el alquiler es 0
        } 
        return alquiler;

    }
}
