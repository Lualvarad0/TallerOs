namespace TallerOS.Domain.OrdenesTrabajo;

public class OrdenTrabajo
{
    public Guid Id { get; private set; }
    public Guid EmpresaId { get; private set; }
    public Guid SucursalId { get; private set; }
    public int Numero { get; private set; }
    public Guid VehiculoId { get; private set; }
    public Guid ClienteId { get; private set; }
    public Guid EstadoId { get; private set; }
    public PrioridadOt Prioridad { get; private set; }
    public int KmIngreso { get; private set; }

    /// <summary>Nulo si todavía no se asigna un asesor.</summary>
    public Guid? AsesorPersonalId { get; private set; }

    public DateTimeOffset IngresoEn { get; private set; }
    public DateTimeOffset? EntregadoEn { get; private set; }
    public DateTimeOffset? EliminadoEn { get; private set; }

    private OrdenTrabajo() { }

    public OrdenTrabajo(
        Guid empresaId,
        Guid sucursalId,
        int numero,
        Guid vehiculoId,
        Guid clienteId,
        Guid estadoId,
        PrioridadOt prioridad,
        int kmIngreso,
        Guid? asesorPersonalId)
    {
        Id = Guid.CreateVersion7();
        EmpresaId = empresaId;
        SucursalId = sucursalId;
        Numero = numero;
        VehiculoId = vehiculoId;
        ClienteId = clienteId;
        EstadoId = estadoId;
        Prioridad = prioridad;
        KmIngreso = kmIngreso;
        AsesorPersonalId = asesorPersonalId;
        IngresoEn = DateTimeOffset.UtcNow;
    }

    /// <summary>Solo mueve el puntero de estado; registrar la fila en historial_estado_ot es
    /// responsabilidad del caso de uso, no de esta entidad. Validar que la transición sea legal
    /// (según el "orden" de estado_ot) también queda para ese caso de uso, que sí conoce el flujo
    /// completo de la empresa.</summary>
    public void CambiarEstado(Guid nuevoEstadoId, bool esEstadoFinal)
    {
        EstadoId = nuevoEstadoId;
        EntregadoEn = esEstadoFinal ? DateTimeOffset.UtcNow : null;
    }

    public void AsignarAsesor(Guid? asesorPersonalId) => AsesorPersonalId = asesorPersonalId;

    public void CambiarPrioridad(PrioridadOt prioridad) => Prioridad = prioridad;
}
