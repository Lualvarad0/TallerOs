using TallerOS.Application.Comun.Cqrs;

namespace TallerOS.Application.Catalogos.TiposVehiculo.Queries.ListarTiposVehiculo;

public sealed record ListarTiposVehiculoQuery : IRequest<IReadOnlyList<TipoVehiculoDto>>;
