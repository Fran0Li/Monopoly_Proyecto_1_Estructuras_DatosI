class Propiedad//: Casilla
{
    public int Precio;
    public int Alquiler;
    public Jugador? Propietario;
    public bool Hipotecada;

    public Propiedad(int Precio, int Alquiler)//: base(int Id, string Nombre, int Posicion)
    {
       this.Precio = Precio;
       this.Alquiler = Alquiler;
       this.Propietario = null;
       this.Hipotecada = false; 
    }
    public void AsignarPropietario(Jugador jugador)
    {
        this.Propietario = jugador;
    }
}