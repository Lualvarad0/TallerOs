using TallerOS.Application.Comun.Cqrs;

namespace TallerOS.Application.Organizacion.Areas.Commands.CrearArea;

public sealed record CrearAreaCommand(Guid SucursalId, string Nombre) : IRequest<Guid>;
