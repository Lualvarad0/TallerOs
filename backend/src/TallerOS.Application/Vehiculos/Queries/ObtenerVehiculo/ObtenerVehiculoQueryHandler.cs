using Microsoft.EntityFrameworkCore;
using TallerOS.Application.Comun;
using TallerOS.Application.Comun.Cqrs;
using TallerOS.Application.Comun.Excepciones;

namespace TallerOS.Application.Vehiculos.Queries.ObtenerVehiculo;

public sealed class ObtenerVehiculoQueryHandler(IAppDbContext db) : IRequestHandler<ObtenerVehiculoQuery, VehiculoDetalleDto>
{
    public async Task<VehiculoDetalleDto> Handle(ObtenerVehiculoQuery request, CancellationToken cancellationToken)
    {
        // El filtro global de Vehiculo (ver AppDbContext) ya exige un ClienteVehiculo propio para
        // verlo: si esta empresa nunca lo vinculó, esto da "no encontrado", igual que con un
        // vehículo que de verdad no existe. No revela que el vehículo es de otra empresa.
        var vehiculoConCatalogo = await (
            from v in db.Vehiculos
            join marca in db.Marcas on v.MarcaId equals marca.Id
            join modelo in db.Modelos on v.ModeloId equals modelo.Id
            where v.Id == request.VehiculoId
            select new { v.Id, v.Vin, v.MarcaId, marca.Nombre, v.ModeloId, ModeloNombre = modelo.Nombre, v.Anio, v.Motor })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new EntidadNoEncontradaException("Vehiculo", request.VehiculoId);

        // Solo la activa (Hasta == null), no el historial: ver el comentario de VehiculoDetalleDto.
        var placaActiva = await db.VehiculoPlacas
            .Where(p => p.VehiculoId == request.VehiculoId && p.Hasta == null)
            .Select(p => p.Placa)
            .FirstOrDefaultAsync(cancellationToken);

        var clientes = await (
            from cv in db.ClienteVehiculos
            join c in db.Clientes on cv.ClienteId equals c.Id
            where cv.VehiculoId == request.VehiculoId
            select new ClienteVinculadoDto(c.Id, c.Nombre, cv.Rol))
            .ToListAsync(cancellationToken);

        return new VehiculoDetalleDto(
            vehiculoConCatalogo.Id, vehiculoConCatalogo.Vin, vehiculoConCatalogo.MarcaId, vehiculoConCatalogo.Nombre,
            vehiculoConCatalogo.ModeloId, vehiculoConCatalogo.ModeloNombre, vehiculoConCatalogo.Anio, vehiculoConCatalogo.Motor,
            placaActiva, clientes);
    }
}
