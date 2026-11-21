namespace TallerOS.Domain.OrdenesTrabajo;

/// <summary>Fila de auditoría de un cambio de estado. Append-only: no tiene métodos de edición ni
/// borrado lógico, y el caso de uso nunca la actualiza, solo la inserta.</summary>
public class HistorialEstadoOt
{
    public Guid Id { get; private set; }
    public Guid EmpresaId { get; private set; }
    public Guid OtId { get; private set; }

    /// <summary>Nulo solo en la primera fila de la OT (el estado inicial no viene "de" ningún otro).</summary>
    public Guid? DeEstadoId { get; private set; }

    public Guid AEstadoId { get; private set; }
    public Guid UsuarioId { get; private set; }
    public DateTimeOffset Fecha { get; private set; }

    private HistorialEstadoOt() { }

    public HistorialEstadoOt(Guid empresaId, Guid otId, Guid? deEstadoId, Guid aEstadoId, Guid usuarioId)
    {
        Id = Guid.CreateVersion7();
        EmpresaId = empresaId;
        OtId = otId;
        DeEstadoId = deEstadoId;
        AEstadoId = aEstadoId;
        UsuarioId = usuarioId;
        Fecha = DateTimeOffset.UtcNow;
    }
}
