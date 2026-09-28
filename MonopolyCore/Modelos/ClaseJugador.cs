using MonopolyCore.Estructuras;

namespace MonopolyCore.Modelos{
public class Jugador
{
    //FIX TEMPORAL
    public int ObtenerId()
        {
            return Id;
        }


    //Identificacion
    private int Id; //identificador unico del jugador
    private string Nombre; //nombre del jugador
    private string TarjetaRfid; //tarjeta RFID del jugador
    //Estados
    private int Saldo; //saldo del jugador
    private int PosicionActual; //posicion actual del jugador
    private bool Activo; //estado de activo del jugador
    private ListaDoblementeEnlazada<Propiedad> Propiedades; //lista de propiedades del jugador
    private int TurnosEnCarcel; //cantidad de turnos que el jugador ha estado en la carcel

//getters y setters
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
            get {return PosicionActual;}
            set {PosicionActual = value;}
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
//constructor de la clase jugador, recibe el id y nombre del jugador
    public Jugador(int Id, string Nombre)
    {
        this.Id = Id;
        this.Nombre = Nombre;
        this.Saldo = 0;
        this.PosicionActual = 1;
        this.Activo = false;
        this.TarjetaRfid = "";
        this.Propiedades = new ListaDoblementeEnlazada<Propiedad>();
        this.TurnosEnCarcel = 0;
    }
    //metodo para calcular el patrimonio total del jugador, sumando el saldo y el valor de las propiedades
    public int PatrimonioTotal()
    {
        int patrimonio = this.Saldo;
        foreach (Propiedad propiedad in Propiedades.RecorrerDesdeInicio())
            {
                patrimonio += propiedad.Precio;
            }
        return patrimonio;
    }
    //metodo para pagar una cantidad de dinero al jugador, disminuyendo su saldo
    public void AvanzarCasilla()
    {
        this.PosicionActual += 1;
    }
    //metodo para retroceder una cantidad de dinero al jugador, disminuyendo su saldo
    public void AgregarPropiedad(Propiedad propiedad)
    {
        Propiedades.AgregarAlFinal(propiedad);
    }
}
}