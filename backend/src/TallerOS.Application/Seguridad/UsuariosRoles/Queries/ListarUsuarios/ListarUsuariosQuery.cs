using TallerOS.Application.Comun.Cqrs;

namespace TallerOS.Application.Seguridad.UsuariosRoles.Queries.ListarUsuarios;

public sealed record ListarUsuariosQuery : IRequest<IReadOnlyList<UsuarioRolDto>>;
