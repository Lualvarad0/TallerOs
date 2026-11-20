using TallerOS.Application.Comun.Cqrs;

namespace TallerOS.Application.Organizacion.Areas.Commands.DesactivarArea;

public sealed record DesactivarAreaCommand(Guid AreaId) : IRequest<Unit>;
