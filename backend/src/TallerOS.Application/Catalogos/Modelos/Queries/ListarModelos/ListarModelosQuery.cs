using TallerOS.Application.Comun.Cqrs;

namespace TallerOS.Application.Catalogos.Modelos.Queries.ListarModelos;

public sealed record ListarModelosQuery(Guid MarcaId) : IRequest<IReadOnlyList<ModeloDto>>;
