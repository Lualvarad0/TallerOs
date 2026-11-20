using Microsoft.EntityFrameworkCore;
using TallerOS.Application.Comun;
using TallerOS.Application.Comun.Cqrs;
using TallerOS.Application.Comun.Excepciones;

namespace TallerOS.Application.Clientes.Queries.ObtenerCliente;

public sealed class ObtenerClienteQueryHandler(IAppDbContext db) : IRequestHandler<ObtenerClienteQuery, ClienteDetalleDto>
{
    public async Task<ClienteDetalleDto> Handle(ObtenerClienteQuery request, CancellationToken cancellationToken)
    {
        var cliente = await db.Clientes.FirstOrDefaultAsync(c => c.Id == request.ClienteId && c.EliminadoEn == null, cancellationToken)
            ?? throw new EntidadNoEncontradaException("Cliente", request.ClienteId);

        // Vehiculo no tiene navegación propia (ver IAppDbContext): se junta a mano con Marca/Modelo.
        // El filtro global de ClienteVehiculo ya acota esto a los vínculos de la empresa de la
        // sesión, así que no hace falta repetir ese chequeo aquí.
        var vehiculos = await (
            from cv in db.ClienteVehiculos
            join v in db.Vehiculos on cv.VehiculoId equals v.Id
            join marca in db.Marcas on v.MarcaId equals marca.Id
            join modelo in db.Modelos on v.ModeloId equals modelo.Id
            where cv.ClienteId == request.ClienteId
            orderby marca.Nombre, modelo.Nombre
            select new { v.Id, v.Vin, v.MarcaId, marca.Nombre, v.ModeloId, ModeloNombre = modelo.Nombre, v.Anio, cv.Rol })
            .ToListAsync(cancellationToken);

        // La placa activa vive aparte (VehiculoPlaca puede tener varias filas por vehículo a lo
        // largo del tiempo, ver RegistrarVehiculoCommand); se trae en una segunda consulta en vez de
        // un tercer join para no duplicar filas de vehiculos si algún día se permite más de una
        // placa "activa" temporalmente.
        var vehiculoIds = vehiculos.Select(v => v.Id).ToList();
        var placasActivas = await db.VehiculoPlacas
            .Where(p => vehiculoIds.Contains(p.VehiculoId) && p.Hasta == null)
            .ToDictionaryAsync(p => p.VehiculoId, p => p.Placa, cancellationToken);

        var vehiculosDto = vehiculos
            .Select(v => new VehiculoResumenDto(
                v.Id, v.Vin, placasActivas.GetValueOrDefault(v.Id), v.MarcaId, v.Nombre, v.ModeloId, v.ModeloNombre, v.Anio, v.Rol))
            .ToList();

        return new ClienteDetalleDto(
            cliente.Id, cliente.Nombre, cliente.Tipo, cliente.Identificacion, cliente.Telefono, cliente.Email,
            cliente.CanalPreferido, vehiculosDto);
    }
}
