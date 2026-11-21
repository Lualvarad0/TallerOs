using TallerOS.Application.Comun.Cqrs;
using TallerOS.Domain.OrdenesTrabajo;

namespace TallerOS.Application.OrdenesTrabajo.Commands.RegistrarRecepcion;

public sealed record DanioPrevioInput(string Zona, TipoDanio Tipo, string? Descripcion, string? FotoUrl);

public sealed record RegistrarRecepcionCommand(
    Guid SucursalId,
    Guid VehiculoId,
    Guid ClienteId,
    PrioridadOt Prioridad,
    int KmIngreso,
    Guid? AsesorPersonalId,
    short Combustible,
    string? Objetos,
    string? Observaciones,
    IReadOnlyList<DanioPrevioInput> DaniosPrevios,
    Guid UsuarioId) : IRequest<RegistrarRecepcionResultado>;

public sealed record RegistrarRecepcionResultado(Guid OtId, int Numero, Guid EstadoId);
