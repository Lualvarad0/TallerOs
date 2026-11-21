using TallerOS.Application.Comun.Cqrs;

namespace TallerOS.Application.OrdenesTrabajo.TiposTrabajo.Queries.ListarTiposTrabajo;

public sealed record ListarTiposTrabajoQuery : IRequest<IReadOnlyList<TipoTrabajoDto>>;
