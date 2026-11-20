using TallerOS.Application.Comun.Cqrs;

namespace TallerOS.Application.Seguridad.UsuariosRoles.Commands.QuitarRol;

public sealed record QuitarRolCommand(Guid UsuarioRolId) : IRequest<Unit>;
