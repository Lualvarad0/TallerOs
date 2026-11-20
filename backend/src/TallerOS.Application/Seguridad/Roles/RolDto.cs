namespace TallerOS.Application.Seguridad.Roles;

public sealed record RolDto(Guid Id, string Nombre, IReadOnlyList<string> Permisos);

public sealed record PermisoDto(string Codigo, string Nombre, string? Descripcion);
