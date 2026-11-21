using TallerOS.Application.Comun.Cqrs;

namespace TallerOS.Application.OrdenesTrabajo.Subtareas.Commands.CrearSubtarea;

public sealed record CrearSubtareaCommand(
    Guid OtId,
    Guid TipoTrabajoId,
    Guid? ResponsablePersonalId,
    int TiempoEstimadoMin,
    short Orden) : IRequest<Guid>;
