namespace TallerOS.Application.OrdenesTrabajo.Queries.ListarEstadosOt;

public sealed record EstadoOtDto(Guid Id, string Nombre, short Orden, bool EsFinal, string ColorToken);
