using TallerOS.Application.Comun.Cqrs;

namespace TallerOS.Application.OrdenesTrabajo.Queries.ObtenerOrdenTrabajo;

public sealed record ObtenerOrdenTrabajoQuery(Guid OtId) : IRequest<OrdenTrabajoDetalleDto>;
