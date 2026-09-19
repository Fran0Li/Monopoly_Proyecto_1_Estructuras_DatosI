namespace MonopolyCore.Modelos{
public class Jugador
{
    //Identificacion
    private int Id;
    public string Nombre;
    public string TarjetaRfid;
    //Estados
    public int Saldo;
    public int PosicionActual;
    public bool Activo;
    //public propiedades; #Aun no listo porque no se como hacer la estructura de datos
    public int TurnosEnCarcel;

    public Jugador(int Id, string Nombre)
    {
        this.Id = Id;
        this.Nombre = Nombre;
        this.Saldo = 0;
        this.PosicionActual = 1;
        this.Activo = false;
        this.TarjetaRfid = "1";
        this.TurnosEnCarcel = 0;
    }
    public int PatrimonioTotal()
    {
        return this.Saldo;
    }
    public void AvanzarCasilla()
    {
        this.PosicionActual += 1;
        Console.WriteLine($"El jugador {this.Nombre}, ahora esta en la casilla {this.PosicionActual}");
    }
}
}