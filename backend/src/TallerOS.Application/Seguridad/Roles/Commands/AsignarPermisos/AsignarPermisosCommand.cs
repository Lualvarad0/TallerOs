using TallerOS.Application.Comun.Cqrs;

namespace TallerOS.Application.Seguridad.Roles.Commands.AsignarPermisos;

/// <summary>Reemplaza el conjunto completo de permisos del rol por Codigos (no es un "agregar
/// permiso"): más simple de razonar desde el frontend, que siempre manda la lista completa marcada.</summary>
public sealed record AsignarPermisosCommand(Guid RolId, IReadOnlyList<string> Codigos) : IRequest<Unit>;
