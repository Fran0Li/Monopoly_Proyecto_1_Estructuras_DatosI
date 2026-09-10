namespace MonopolyCore.Comunicacion
{
    //Clases que definen la estructura de los datos enviados y recibidos por el hardware
    public class DatosConexionHardware  //Datos usados para identificar la conexión como hardware
    {
        public string TipoCliente { get; set; } = "Hardware";
    }

    public class DatosRfid //Datos enviados cuando se detecta una tarjeta RFID
    {
        public string UID { get; set; } = "";
    }

    public class DatosDados //Datos enviados al hardware para mostrar el resultado de los dados
    {
        public int Valor1 { get; set; }
        public int Valor2 { get; set; }
    }
}