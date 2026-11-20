using TallerOS.Application.Comun.Cqrs;

namespace TallerOS.Application.Organizacion.Sucursales.Queries.ListarSucursales;

public sealed record ListarSucursalesQuery : IRequest<IReadOnlyList<SucursalDto>>;
