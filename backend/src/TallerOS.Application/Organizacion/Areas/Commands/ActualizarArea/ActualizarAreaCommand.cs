using TallerOS.Application.Comun.Cqrs;

namespace TallerOS.Application.Organizacion.Areas.Commands.ActualizarArea;

public sealed record ActualizarAreaCommand(Guid AreaId, string Nombre) : IRequest<Unit>;
