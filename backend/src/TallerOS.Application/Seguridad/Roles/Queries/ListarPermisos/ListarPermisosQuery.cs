using TallerOS.Application.Comun.Cqrs;

namespace TallerOS.Application.Seguridad.Roles.Queries.ListarPermisos;

public sealed record ListarPermisosQuery : IRequest<IReadOnlyList<PermisoDto>>;
