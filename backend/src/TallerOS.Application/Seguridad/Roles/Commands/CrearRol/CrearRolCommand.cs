using TallerOS.Application.Comun.Cqrs;

namespace TallerOS.Application.Seguridad.Roles.Commands.CrearRol;

public sealed record CrearRolCommand(string Nombre) : IRequest<Guid>;
