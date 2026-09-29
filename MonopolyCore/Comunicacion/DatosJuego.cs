namespace MonopolyCore.Comunicacion
{
    // DTOs que viajan en el campo Datos. La GUI puede deserializarlos directamente con
    // JsonSerializer.Deserialize<EstadoJuegoDto>(mensaje.Datos.Value.GetRawText()).
    // Solo arreglos: nada de List.

    public class EstadoJuegoDto
    {
        public string Estado { get; set; } = "";           //Esperando | EnCurso | Finalizado
        public int NumeroTurno { get; set; }
        public int MaxTurnos { get; set; }
        public int? JugadorEnTurnoId { get; set; }
        public bool DadosLanzados { get; set; }
        public int Dado1 { get; set; }
        public int Dado2 { get; set; }
        public int? CompraPendientePosicion { get; set; }  //casilla que el jugador en turno puede comprar
        public PagoPendienteDto? PagoPendiente { get; set; } //pago esperando tarjeta RFID
        public bool RfidParaPagos { get; set; }
        public int? GanadorId { get; set; }
        public int[] OrdenTurnos { get; set; } = new int[0];
        public JugadorDto[] Jugadores { get; set; } = new JugadorDto[0];
        public CasillaDto[] Casillas { get; set; } = new CasillaDto[0];
    }

    public class JugadorDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public int Saldo { get; set; }
        public int Posicion { get; set; }
        public bool Activo { get; set; }
        public bool TieneTarjeta { get; set; }
        public int TurnosPorPerder { get; set; }
        public int Patrimonio { get; set; }
        public int[] Propiedades { get; set; } = new int[0]; //posiciones de sus propiedades
    }

    public class CasillaDto
    {
        public int Posicion { get; set; }
        public string Nombre { get; set; } = "";
        public string Tipo { get; set; } = "";     //Propiedad | Evento | Especial
        public string Subtipo { get; set; } = "";  //solo especiales: Inicio, Carcel, ParqueoGratis, IrACarcel, Impuesto
        public int Precio { get; set; }
        public int Alquiler { get; set; }
        public int Monto { get; set; }             //impuesto
        public int? PropietarioId { get; set; }
    }

    public class PagoPendienteDto
    {
        public int JugadorId { get; set; }
        public int? AcreedorId { get; set; }       //null = Banco
        public int Monto { get; set; }
        public string Descripcion { get; set; } = "";
        public bool EsCompra { get; set; }
    }

    public class TransaccionDto
    {
        public int Id { get; set; }
        public DateTime FechaHora { get; set; }
        public int NumeroTurno { get; set; }
        public string Tipo { get; set; } = "";
        public int? OrigenId { get; set; }
        public string Origen { get; set; } = "";
        public int? DestinoId { get; set; }
        public string Destino { get; set; } = "";
        public int Monto { get; set; }
        public string Descripcion { get; set; } = "";
    }
}
