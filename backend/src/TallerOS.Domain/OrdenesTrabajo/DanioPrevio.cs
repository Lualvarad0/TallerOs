namespace TallerOS.Domain.OrdenesTrabajo;

/// <summary>Un punto marcado sobre la silueta del vehículo (ver docs/diseno/Silueta.dc.html): zona
/// predefinida + tipo de daño, no una anotación libre sobre una foto (eso es para hallazgos, en el
/// Módulo 6).</summary>
public class DanioPrevio
{
    public Guid Id { get; private set; }
    public Guid EmpresaId { get; private set; }
    public Guid RecepcionId { get; private set; }

    /// <summary>Nombre de la zona de la silueta (p. ej. "puerta_delantera_izquierda"), no coordenadas
    /// libres de un canvas.</summary>
    public string Zona { get; private set; } = string.Empty;

    public TipoDanio Tipo { get; private set; }
    public string? Descripcion { get; private set; }
    public string? FotoUrl { get; private set; }

    private DanioPrevio() { }

    public DanioPrevio(Guid empresaId, Guid recepcionId, string zona, TipoDanio tipo, string? descripcion, string? fotoUrl)
    {
        Id = Guid.CreateVersion7();
        EmpresaId = empresaId;
        RecepcionId = recepcionId;
        Zona = zona;
        Tipo = tipo;
        Descripcion = descripcion;
        FotoUrl = fotoUrl;
    }
}
