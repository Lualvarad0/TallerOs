using TallerOS.Application.Comun.Cqrs;

namespace TallerOS.Application.Organizacion.Areas.Queries.ListarAreas;

public sealed record ListarAreasQuery(Guid SucursalId) : IRequest<IReadOnlyList<AreaDto>>;
