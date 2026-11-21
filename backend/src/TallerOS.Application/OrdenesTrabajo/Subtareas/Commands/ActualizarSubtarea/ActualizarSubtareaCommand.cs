using TallerOS.Application.Comun.Cqrs;
using TallerOS.Domain.OrdenesTrabajo;

namespace TallerOS.Application.OrdenesTrabajo.Subtareas.Commands.ActualizarSubtarea;

public sealed record ActualizarSubtareaCommand(Guid SubtareaId, Guid? ResponsablePersonalId, EstadoSubtarea Estado) : IRequest<Unit>;
