namespace TallerOS.Domain.OrdenesTrabajo;

/// <summary>Catálogo de trabajos por empresa (p. ej. "Cambio de aceite"). IntervaloKm/IntervaloMeses
/// se guardan desde ya porque son columnas de este mismo catálogo, pero nadie los lee todavía: los
/// usará el Módulo 6 (recordatorios de mantenimiento).</summary>
public class TipoTrabajo
{
    public Guid Id { get; private set; }
    public Guid EmpresaId { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public int TiempoEstandarMin { get; private set; }
    public int? IntervaloKm { get; private set; }
    public int? IntervaloMeses { get; private set; }
    public DateTimeOffset? EliminadoEn { get; private set; }

    private TipoTrabajo() { }

    public TipoTrabajo(Guid empresaId, string nombre, int tiempoEstandarMin, int? intervaloKm, int? intervaloMeses)
    {
        Id = Guid.CreateVersion7();
        EmpresaId = empresaId;
        Nombre = nombre;
        TiempoEstandarMin = tiempoEstandarMin;
        IntervaloKm = intervaloKm;
        IntervaloMeses = intervaloMeses;
    }

    public void ActualizarDatos(string nombre, int tiempoEstandarMin, int? intervaloKm, int? intervaloMeses)
    {
        Nombre = nombre;
        TiempoEstandarMin = tiempoEstandarMin;
        IntervaloKm = intervaloKm;
        IntervaloMeses = intervaloMeses;
    }

    public void Desactivar() => EliminadoEn = DateTimeOffset.UtcNow;
}
