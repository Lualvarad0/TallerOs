using TallerOS.Application.Comun.Cqrs;

namespace TallerOS.Application.Catalogos.Marcas.Queries.ListarMarcas;

public sealed record ListarMarcasQuery(short? TipoVehiculoId) : IRequest<IReadOnlyList<MarcaDto>>;
