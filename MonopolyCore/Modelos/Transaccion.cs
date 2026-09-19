namespace MonopolyCore.Modelos{
    
public class Transaccion
    {
        public int Id {get; set;}
        public DateTime FechaHora {get; set;}
        public int NumeroTurno {get; set;}
        public TipoTransaccion Tipo {get; set;}
        public int? JugadorOrigenId {get; set;}
        public int? JugadorDestinoId {get; set;}
        public int Monto {get; set;}
        public string? Descripcion {get; set;}

        public Transaccion(int Id, int NumeroTurno, TipoTransaccion Tipo, int? JugadorOrigenId, int? JugadorDestinoId,int Monto,string? Descripcion)
        {
            this.Id = Id;
            this.FechaHora = DateTime.Now;
            this.NumeroTurno = NumeroTurno;
            this.Tipo = Tipo;
            this.JugadorOrigenId = JugadorOrigenId;
            this.JugadorDestinoId = JugadorDestinoId;   
            this.Monto = Monto;
            this.Descripcion = Descripcion;
        }
    }    

}
