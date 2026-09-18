namespace MonopolyCore.Modelos{
public class Jugador
{
    //Identificacion
    private int Id;
    private string Nombre;
    private string TarjetaRfid;
    //Estados
    private int Saldo;
    private int PosicionActual;
    private bool Activo;
    //public propiedades; #Aun no listo porque no se como hacer la estructura de datos
    private int TurnosEnCarcel;

    public int id
        {
            get {return Id;}
            set {Id = value;}
        }
    public string nombre
        {
            get {return Nombre;}
            set {Nombre = value;}
        }
    public string tarjetaRfid
        {
            get {return TarjetaRfid;}
            set {TarjetaRfid= value;}
        }
    public int saldo
        {
            get {return Saldo;}
            set {Saldo = value;}
        }
    public int posicionActual
        {
            get {return Saldo;}
            set {Saldo = value;}
        }
    public bool activo
        {
            get {return Activo;}
            set {Activo = value;}
        }
    public int turnosEnCarcel
        {
            get {return TurnosEnCarcel;}
            set {TurnosEnCarcel = value;}
        }

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