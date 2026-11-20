using TallerOS.Application.Comun.Cqrs;

namespace TallerOS.Application.Seguridad.UsuariosRoles.Commands.InvitarUsuario;

/// <summary>Da acceso a una persona a esta empresa con un rol. Si el correo ya tiene cuenta (en
/// esta empresa o en otra: usuario es global), se reutiliza esa cuenta y Password se ignora; si no
/// existe, se crea con Password. SucursalId nulo = el rol aplica a todas las sucursales.</summary>
public sealed record InvitarUsuarioCommand(string Email, string? Password, Guid RolId, Guid? SucursalId) : IRequest<Guid>;
