namespace TallerOS.Domain.Plataforma;

public class Suscripcion
{
    public Guid Id { get; private set; }
    public Guid EmpresaId { get; private set; }
    public Guid PlanId { get; private set; }
    public EstadoSuscripcion Estado { get; private set; }
    public DateTimeOffset VigenteDesde { get; private set; }
    public DateTimeOffset? VigenteHasta { get; private set; }

    private Suscripcion() { }

    public Suscripcion(Guid empresaId, Guid planId, DateTimeOffset vigenteDesde)
    {
        Id = Guid.CreateVersion7();
        EmpresaId = empresaId;
        PlanId = planId;
        Estado = EstadoSuscripcion.Activa;
        VigenteDesde = vigenteDesde;
    }
}
