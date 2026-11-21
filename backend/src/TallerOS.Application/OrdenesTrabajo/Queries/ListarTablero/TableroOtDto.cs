using TallerOS.Domain.OrdenesTrabajo;

namespace TallerOS.Application.OrdenesTrabajo.Queries.ListarTablero;

/// <summary>Lo que pinta una tarjeta del Kanban. Placa nula es posible (vehículo sin placa activa
/// registrada todavía); el resto de los datos del vehículo (marca, modelo) no van aquí, se piden en
/// ObtenerOrdenTrabajo cuando se abre el detalle.</summary>
public sealed record TableroOtDto(
    Guid OtId,
    int Numero,
    string? Placa,
    string ClienteNombre,
    PrioridadOt Prioridad,
    Guid EstadoId,
    string EstadoNombre,
    string ColorToken,
    DateTimeOffset IngresoEn);
