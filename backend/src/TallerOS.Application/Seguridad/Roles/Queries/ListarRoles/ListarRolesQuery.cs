using TallerOS.Application.Comun.Cqrs;

namespace TallerOS.Application.Seguridad.Roles.Queries.ListarRoles;

public sealed record ListarRolesQuery : IRequest<IReadOnlyList<RolDto>>;
