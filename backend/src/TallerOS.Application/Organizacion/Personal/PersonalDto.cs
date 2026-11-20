namespace TallerOS.Application.Organizacion.Personal;

public sealed record PersonalDto(Guid Id, Guid AreaId, string Nombre, decimal? TarifaHora, bool TieneAcceso);
