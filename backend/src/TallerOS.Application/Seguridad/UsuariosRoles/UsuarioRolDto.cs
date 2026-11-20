namespace TallerOS.Application.Seguridad.UsuariosRoles;

public sealed record UsuarioRolDto(Guid Id, Guid UsuarioId, string Email, Guid RolId, string Rol, Guid? SucursalId);
