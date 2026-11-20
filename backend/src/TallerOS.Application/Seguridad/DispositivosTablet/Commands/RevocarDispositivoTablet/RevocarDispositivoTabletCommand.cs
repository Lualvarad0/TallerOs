using TallerOS.Application.Comun.Cqrs;

namespace TallerOS.Application.Seguridad.DispositivosTablet.Commands.RevocarDispositivoTablet;

public sealed record RevocarDispositivoTabletCommand(Guid DispositivoId) : IRequest<Unit>;
