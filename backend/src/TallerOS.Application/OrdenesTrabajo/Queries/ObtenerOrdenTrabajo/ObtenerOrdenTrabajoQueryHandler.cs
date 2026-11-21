using Microsoft.EntityFrameworkCore;
using TallerOS.Application.Comun;
using TallerOS.Application.Comun.Cqrs;
using TallerOS.Application.Comun.Excepciones;

namespace TallerOS.Application.OrdenesTrabajo.Queries.ObtenerOrdenTrabajo;

public sealed class ObtenerOrdenTrabajoQueryHandler(IAppDbContext db) : IRequestHandler<ObtenerOrdenTrabajoQuery, OrdenTrabajoDetalleDto>
{
    public async Task<OrdenTrabajoDetalleDto> Handle(ObtenerOrdenTrabajoQuery request, CancellationToken cancellationToken)
    {
        var cabecera = await (
            from ot in db.OrdenesTrabajo
            where ot.Id == request.OtId
            join estado in db.EstadosOt on ot.EstadoId equals estado.Id
            join cliente in db.Clientes on ot.ClienteId equals cliente.Id
            select new
            {
                ot.Id, ot.Numero, ot.SucursalId, ot.VehiculoId, ot.ClienteId, Cliente = cliente.Nombre,
                EstadoId = estado.Id, Estado = estado.Nombre, ot.Prioridad, ot.KmIngreso, ot.AsesorPersonalId,
                ot.IngresoEn, ot.EntregadoEn,
            }).FirstOrDefaultAsync(cancellationToken)
            ?? throw new EntidadNoEncontradaException("Orden de trabajo", request.OtId);

        var recepcion = await db.Recepciones
            .Where(r => r.OtId == request.OtId)
            .Select(r => new RecepcionDto(r.Combustible, r.Objetos, r.Observaciones, r.FirmaUrl, Array.Empty<DanioPrevioDto>()))
            .FirstOrDefaultAsync(cancellationToken);

        if (recepcion is not null)
        {
            var daniosPrevios = await db.DaniosPrevios
                .Where(d => db.Recepciones.Any(r => r.OtId == request.OtId && r.Id == d.RecepcionId))
                .Select(d => new DanioPrevioDto(d.Id, d.Zona, d.Tipo, d.Descripcion, d.FotoUrl))
                .ToListAsync(cancellationToken);
            recepcion = recepcion with { DaniosPrevios = daniosPrevios };
        }

        var subtareas = await (
            from s in db.Subtareas
            where s.OtId == request.OtId
            join tipoTrabajo in db.TiposTrabajo on s.TipoTrabajoId equals tipoTrabajo.Id
            orderby s.Orden
            select new SubtareaDto(s.Id, tipoTrabajo.Id, tipoTrabajo.Nombre, s.ResponsablePersonalId, s.Estado, s.TiempoEstimadoMin, s.Orden)
            ).ToListAsync(cancellationToken);

        var historial = await (
            from h in db.HistorialEstadoOt
            where h.OtId == request.OtId
            join estadoDestino in db.EstadosOt on h.AEstadoId equals estadoDestino.Id
            orderby h.Fecha descending
            select new HistorialEstadoOtDto(h.DeEstadoId, h.AEstadoId, estadoDestino.Nombre, h.UsuarioId, h.Fecha)
            ).ToListAsync(cancellationToken);

        return new OrdenTrabajoDetalleDto(
            cabecera.Id, cabecera.Numero, cabecera.SucursalId, cabecera.VehiculoId, cabecera.ClienteId, cabecera.Cliente,
            cabecera.EstadoId, cabecera.Estado, cabecera.Prioridad, cabecera.KmIngreso, cabecera.AsesorPersonalId,
            cabecera.IngresoEn, cabecera.EntregadoEn, recepcion, subtareas, historial);
    }
}
