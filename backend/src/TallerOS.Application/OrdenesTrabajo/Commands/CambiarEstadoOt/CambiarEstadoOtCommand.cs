using TallerOS.Application.Comun.Cqrs;

namespace TallerOS.Application.OrdenesTrabajo.Commands.CambiarEstadoOt;

public sealed record CambiarEstadoOtCommand(Guid OtId, Guid NuevoEstadoId, Guid UsuarioId) : IRequest<Unit>;
