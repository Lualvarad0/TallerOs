using TallerOS.Application.Comun.Cqrs;

namespace TallerOS.Application.Organizacion.Personal.Commands.CrearPersonal;

/// <summary>Pin, si viene, es el PIN en texto plano que llega del cliente: el handler lo hashea,
/// nunca se guarda ni se loguea tal cual.</summary>
public sealed record CrearPersonalCommand(
    Guid AreaId, string Nombre, decimal? TarifaHora, bool TieneAcceso, Guid? UsuarioId, string? Pin) : IRequest<Guid>;
