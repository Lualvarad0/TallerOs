using TallerOS.Application.Comun.Cqrs;

namespace TallerOS.Application.Organizacion.Personal.Commands.ActualizarPersonal;

public sealed record ActualizarPersonalCommand(Guid PersonalId, Guid AreaId, string Nombre, decimal? TarifaHora) : IRequest<Unit>;
