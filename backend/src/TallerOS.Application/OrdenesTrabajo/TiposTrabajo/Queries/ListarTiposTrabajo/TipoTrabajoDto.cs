namespace TallerOS.Application.OrdenesTrabajo.TiposTrabajo.Queries.ListarTiposTrabajo;

public sealed record TipoTrabajoDto(Guid Id, string Nombre, int TiempoEstandarMin, int? IntervaloKm, int? IntervaloMeses);
