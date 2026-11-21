using Microsoft.EntityFrameworkCore;
using TallerOS.Application.Comun;
using TallerOS.Application.Comun.Cqrs;

namespace TallerOS.Application.OrdenesTrabajo.Queries.ListarTablero;

public sealed class ListarTableroQueryHandler(IAppDbContext db) : IRequestHandler<ListarTableroQuery, IReadOnlyList<TableroOtDto>>
{
    public async Task<IReadOnlyList<TableroOtDto>> Handle(ListarTableroQuery request, CancellationToken cancellationToken)
    {
        var ordenes = db.OrdenesTrabajo.Where(o => o.EliminadoEn == null);

        if (request.SucursalId is { } sucursalId)
        {
            ordenes = ordenes.Where(o => o.SucursalId == sucursalId);
        }

        if (request.AsesorPersonalId is { } asesorPersonalId)
        {
            ordenes = ordenes.Where(o => o.AsesorPersonalId == asesorPersonalId);
        }

        if (request.Prioridad is { } prioridad)
        {
            ordenes = ordenes.Where(o => o.Prioridad == prioridad);
        }

        if (request.EstadoId is { } estadoId)
        {
            ordenes = ordenes.Where(o => o.EstadoId == estadoId);
        }

        // Sin navegaciones (ver AppDbContext): joins explícitos. Placa activa por LEFT JOIN porque
        // un vehículo puede, en teoría, no tener ninguna todavía.
        var consulta =
            from ot in ordenes
            join estado in db.EstadosOt on ot.EstadoId equals estado.Id
            join cliente in db.Clientes on ot.ClienteId equals cliente.Id
            join placa in db.VehiculoPlacas.Where(p => p.Hasta == null) on ot.VehiculoId equals placa.VehiculoId into placasDelVehiculo
            from placa in placasDelVehiculo.DefaultIfEmpty()
            orderby estado.Orden, ot.IngresoEn
            select new TableroOtDto(ot.Id, ot.Numero, placa.Placa, cliente.Nombre, ot.Prioridad, estado.Id, estado.Nombre, estado.ColorToken, ot.IngresoEn);

        return await consulta.ToListAsync(cancellationToken);
    }
}
