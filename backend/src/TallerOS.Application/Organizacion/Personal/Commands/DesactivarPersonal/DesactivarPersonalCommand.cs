using TallerOS.Application.Comun.Cqrs;

namespace TallerOS.Application.Organizacion.Personal.Commands.DesactivarPersonal;

public sealed record DesactivarPersonalCommand(Guid PersonalId) : IRequest<Unit>;
