using TallerOS.Application.Comun.Cqrs;

namespace TallerOS.Application.Seguridad.DispositivosTablet.Queries.ListarDispositivosTablet;

public sealed record ListarDispositivosTabletQuery : IRequest<IReadOnlyList<DispositivoTabletDto>>;
