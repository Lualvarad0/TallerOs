namespace TallerOS.Domain.OrdenesTrabajo;

/// <summary>Un trabajo concreto dentro de una OT. A propósito no tiene cronómetro ni campos de
/// tiempo real (inicio/fin): eso es registro_tiempo, del Módulo 5.</summary>
public class Subtarea
{
    public Guid Id { get; private set; }
    public Guid EmpresaId { get; private set; }
    public Guid OtId { get; private set; }
    public Guid TipoTrabajoId { get; private set; }

    /// <summary>Nulo mientras no se asigna a nadie.</summary>
    public Guid? ResponsablePersonalId { get; private set; }

    public EstadoSubtarea Estado { get; private set; }
    public int TiempoEstimadoMin { get; private set; }
    public short Orden { get; private set; }

    private Subtarea() { }

    public Subtarea(Guid empresaId, Guid otId, Guid tipoTrabajoId, Guid? responsablePersonalId, int tiempoEstimadoMin, short orden)
    {
        Id = Guid.CreateVersion7();
        EmpresaId = empresaId;
        OtId = otId;
        TipoTrabajoId = tipoTrabajoId;
        ResponsablePersonalId = responsablePersonalId;
        Estado = EstadoSubtarea.Pendiente;
        TiempoEstimadoMin = tiempoEstimadoMin;
        Orden = orden;
    }

    public void AsignarResponsable(Guid? responsablePersonalId) => ResponsablePersonalId = responsablePersonalId;

    public void CambiarEstado(EstadoSubtarea estado) => Estado = estado;
}
