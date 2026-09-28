namespace MonopolyCore.Modelos{
    
public class Transaccion
// atributos y sus getters y setters
    {
        public int Id {get; set;} //identificador unico de la transaccion
        public DateTime FechaHora {get; set;}//fecha y hora de la transaccion
        public int NumeroTurno {get; set;}//numero de turno en el que se realizo la transaccion
        public TipoTransaccion Tipo {get; set;}//tipo de transaccion (compra, venta, pago, cobro, etc.)
        public int? JugadorOrigenId {get; set;}//identificador del jugador que realiza la transaccion
        public int? JugadorDestinoId {get; set;}//identificador del jugador que recibe la transaccion
        public int Monto {get; set;}//monto de la transaccion
        public string? Descripcion {get; set;}//descripcion de la transaccion
//constructor de la clase transaccion, recibe el id, numero de turno, tipo de transaccion, id del jugador origen, id del jugador destino, monto y descripcion
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
