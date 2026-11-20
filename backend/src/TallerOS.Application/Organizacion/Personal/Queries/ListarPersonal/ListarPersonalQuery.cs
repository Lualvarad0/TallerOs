using TallerOS.Application.Comun.Cqrs;

namespace TallerOS.Application.Organizacion.Personal.Queries.ListarPersonal;

public sealed record ListarPersonalQuery(Guid AreaId) : IRequest<IReadOnlyList<PersonalDto>>;
