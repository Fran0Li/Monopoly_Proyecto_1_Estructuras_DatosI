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
    private int Id;
    private string Nombre;
    private string TarjetaRfid;
    //Estados
    private int Saldo;
    private int PosicionActual;
    private bool Activo;
    private ListaDoblementeEnlazada<Propiedad> Propiedades; 
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
        this.Propiedades = new ListaDoblementeEnlazada<Propiedad>();
        this.TurnosEnCarcel = 0;
    }
    public int PatrimonioTotal()
    {
        int patrimonio = this.Saldo;
        foreach (Propiedad propiedad in Propiedades.RecorrerDesdeInicio())
            {
                patrimonio += propiedad.Precio;
            }
        return patrimonio;
    }
    public void AvanzarCasilla()
    {
        this.PosicionActual += 1;
    }
    public void AgregarPropiedad(Propiedad propiedad)
    {
        Propiedades.AgregarAlFinal(propiedad);
    }
}
}