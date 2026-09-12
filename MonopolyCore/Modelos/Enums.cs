public enum TipoCasilla
{
    Propiedad,
    Evento,
    Especial,   
}
public enum TipoCasillaEspecial
{
    Inicio,
    Carcel,
    ParqueoGratis,
    IrACarcel,
    Impuesto
}

public enum TipoEfectoEvento
{
    GanarDinero,
    PerderDinero,
    Moverse,
    IrACarcel,
    SalirDeCarcelGratis
}

public enum TipoTransaccion
{
    CompraPropiedad,
    PagoAlBanco,
    PagoAlquiler,
    PagoEntreJugadores,
    GananciaEvento,
    PerdidaEvento,
    PremioPorPasarInicio
}

public enum EstadoJuego
{
    Esperando,
    EnCurso,
    Finalizado
}