using TallerOS.Application.Comun.Cqrs;
using TallerOS.Domain.OrdenesTrabajo;

namespace TallerOS.Application.OrdenesTrabajo.Queries.ListarTablero;

public sealed record ListarTableroQuery(
    Guid? SucursalId,
    Guid? AsesorPersonalId,
    PrioridadOt? Prioridad,
    Guid? EstadoId) : IRequest<IReadOnlyList<TableroOtDto>>;
