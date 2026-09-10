using MonopolyServidor.Comunicacion;

ServidorTcp servidor = new ServidorTcp(5000);

await servidor.IniciarAsync();


public static class CodigosError
{
    public const string FueraDeTurno = "FUERA_DE_TURNO";
    public const string SaldoInsuficiente = "SALDO_INSUFICIENTE";
    public const string PropiedadYaVendida = "PROPIEDAD_YA_VENDIDA";
    public const string DadosYaLanzados = "DADOS_YA_LANZADOS";
    public const string JugadorNoEncontrado = "JUGADOR_NO_ENCONTRADO";
    public const string AccionInvalida = "ACCION_INVALIDA";
    public const string JugadorEliminado = "JUGADOR_ELIMINADO";
}