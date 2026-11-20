namespace TallerOS.Application.Seguridad.DispositivosTablet;

public sealed record DispositivoTabletDto(Guid Id, Guid SucursalId, string Nombre, bool Activo, DateTimeOffset? UltimoUsoEn);
