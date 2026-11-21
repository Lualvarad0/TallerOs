using TallerOS.Domain.OrdenesTrabajo;

namespace TallerOS.Application.OrdenesTrabajo.Queries.ObtenerOrdenTrabajo;

public sealed record DanioPrevioDto(Guid Id, string Zona, TipoDanio Tipo, string? Descripcion, string? FotoUrl);

public sealed record RecepcionDto(
    short Combustible, string? Objetos, string? Observaciones, string? FirmaUrl, IReadOnlyList<DanioPrevioDto> DaniosPrevios);

public sealed record SubtareaDto(
    Guid Id, Guid TipoTrabajoId, string TipoTrabajoNombre, Guid? ResponsablePersonalId, EstadoSubtarea Estado, int TiempoEstimadoMin, short Orden);

public sealed record HistorialEstadoOtDto(Guid? DeEstadoId, Guid AEstadoId, string AEstadoNombre, Guid UsuarioId, DateTimeOffset Fecha);

public sealed record OrdenTrabajoDetalleDto(
    Guid Id,
    int Numero,
    Guid SucursalId,
    Guid VehiculoId,
    Guid ClienteId,
    string ClienteNombre,
    Guid EstadoId,
    string EstadoNombre,
    PrioridadOt Prioridad,
    int KmIngreso,
    Guid? AsesorPersonalId,
    DateTimeOffset IngresoEn,
    DateTimeOffset? EntregadoEn,
    RecepcionDto? Recepcion,
    IReadOnlyList<SubtareaDto> Subtareas,
    IReadOnlyList<HistorialEstadoOtDto> Historial);
