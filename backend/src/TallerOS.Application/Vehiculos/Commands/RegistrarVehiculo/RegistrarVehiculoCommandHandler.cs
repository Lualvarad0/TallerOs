using Microsoft.EntityFrameworkCore;
using TallerOS.Application.Comun;
using TallerOS.Application.Comun.Cqrs;
using TallerOS.Application.Comun.Excepciones;
using TallerOS.Domain.Validators;
using TallerOS.Domain.Vehiculos;

namespace TallerOS.Application.Vehiculos.Commands.RegistrarVehiculo;

public sealed class RegistrarVehiculoCommandHandler(IAppDbContext db, ITenantContext tenant)
    : IRequestHandler<RegistrarVehiculoCommand, RegistrarVehiculoResultado>
{
    public async Task<RegistrarVehiculoResultado> Handle(RegistrarVehiculoCommand request, CancellationToken cancellationToken)
    {
        var empresaId = tenant.EmpresaId ?? throw new SinContextoDeEmpresaException();

        // El filtro global de EF Core (AppDbContext) ya acota Clientes a esta empresa; este chequeo
        // explícito es la segunda verificación que pide este comando, para no depender solo de ese
        // filtro (si algún día se rompiera, este handler seguiría negando el vínculo).
        var cliente = await db.Clientes
            .FirstOrDefaultAsync(c => c.Id == request.ClienteId && c.EliminadoEn == null, cancellationToken);
        if (cliente is null || cliente.EmpresaId != empresaId)
        {
            throw new EntidadNoEncontradaException("Cliente", request.ClienteId);
        }

        if (!await db.Marcas.AnyAsync(m => m.Id == request.MarcaId, cancellationToken))
        {
            throw new EntidadNoEncontradaException("Marca", request.MarcaId);
        }

        var modelo = await db.Modelos.FirstOrDefaultAsync(m => m.Id == request.ModeloId, cancellationToken)
            ?? throw new EntidadNoEncontradaException("Modelo", request.ModeloId);
        if (modelo.MarcaId != request.MarcaId)
        {
            throw new ConflictoDeNegocioException("El modelo indicado no pertenece a la marca indicada.");
        }

        var vin = request.Vin?.Trim().ToUpperInvariant();
        var placa = request.Placa?.Trim().ToUpperInvariant();

        // Primero busca solo entre los vehículos que ESTA empresa ya puede ver (el filtro global de
        // Vehiculo/VehiculoPlaca en AppDbContext ya los acota a los que tienen un ClienteVehiculo
        // propio): es la consulta más barata y cubre el caso normal (cliente repetido, flota propia).
        var vehiculoPorVin = vin is null
            ? null
            : await db.Vehiculos.FirstOrDefaultAsync(v => v.Vin == vin, cancellationToken);

        var vehiculoPorPlaca = placa is null
            ? null
            : await BuscarPorPlacaActivaAsync(db, placa, global: false, cancellationToken);

        // Búsqueda global por identificador, no "solo si ninguno apareció": si el VIN ya coincidió
        // con un vehículo que esta empresa conoce pero la placa dada todavía no apareció localmente,
        // la placa podría pertenecer a un vehículo DISTINTO de otra empresa (el caso que esta
        // búsqueda por-identificador existe para detectar, no solo para encontrar un vehículo "nuevo"
        // que reutilizar) — y lo mismo al revés. Buscar solo cuando ambos faltan dejaría pasar esa
        // mezcla sin chocar nunca con el chequeo de conflicto de abajo.
        var faltaVinGlobal = vin is not null && vehiculoPorVin is null;
        var faltaPlacaGlobal = placa is not null && vehiculoPorPlaca is null;
        if (faltaVinGlobal || faltaPlacaGlobal)
        {
            // Lo que falta por resolver localmente se busca una sola vez entre TODOS los vehículos
            // (de cualquier empresa): el mismo vehículo físico pudo pasar antes por otro taller que
            // también usa TallerOS, y conocer su VIN o su placa activa alcanza, por decisión de
            // negocio (ver RegistrarVehiculoCommand), para reutilizarlo en vez de chocar con el
            // índice único al intentar crear uno nuevo. La elevación (ver
            // IAppDbContext.ElevarPrivilegiosSinEmpresaAsync, tercer caso legítimo) solo dura esta
            // búsqueda; lo único que se lee de otra empresa son los datos propios del vehículo, nunca
            // quién lo atendió.
            await db.ElevarPrivilegiosSinEmpresaAsync(cancellationToken);

            if (faltaVinGlobal)
            {
                vehiculoPorVin = await db.Vehiculos.IgnoreQueryFilters().FirstOrDefaultAsync(v => v.Vin == vin, cancellationToken);
            }

            if (faltaPlacaGlobal)
            {
                vehiculoPorPlaca = await BuscarPorPlacaActivaAsync(db, placa!, global: true, cancellationToken);
            }
        }

        if (vehiculoPorVin is not null && vehiculoPorPlaca is not null && vehiculoPorVin.Id != vehiculoPorPlaca.Id)
        {
            throw new ConflictoDeNegocioException("El VIN y la placa indicados corresponden a vehículos distintos.");
        }

        if (vehiculoPorVin is null && vehiculoPorPlaca is not null && vin is not null
            && vehiculoPorPlaca.Vin is not null && vehiculoPorPlaca.Vin != vin)
        {
            throw new ConflictoDeNegocioException("La placa indicada ya pertenece a un vehículo con un VIN distinto.");
        }

        var vehiculo = vehiculoPorVin ?? vehiculoPorPlaca;
        var esNuevo = vehiculo is null;

        if (vehiculo is null)
        {
            vehiculo = new Vehiculo(vin, request.MarcaId, request.ModeloId, request.Anio, request.Motor);
            db.Vehiculos.Add(vehiculo);
        }
        else
        {
            // Vehiculo no tiene ninguna política RLS de UPDATE (ver Modulo3RlsPoliticas: a
            // propósito, tener un vínculo da derecho a consultar, no a editar un dato que la
            // empresa comparte con otras), así que este comando NO puede llamar a
            // Vehiculo.CompletarVin aunque el VIN dado complete uno que antes era nulo: ese UPDATE
            // lo rechazaría Postgres sin importar quién lo pida. Se vincula igual (el vehículo ya
            // era conocido por esta empresa), pero el VIN dado no queda guardado; completar un VIN
            // que ya existía sin él queda para cuando exista un comando con permiso de editarlo.
            var yaVinculado = await db.ClienteVehiculos
                .AnyAsync(cv => cv.ClienteId == request.ClienteId && cv.VehiculoId == vehiculo.Id, cancellationToken);
            if (yaVinculado)
            {
                throw new ConflictoDeNegocioException("Este cliente ya está vinculado a este vehículo.");
            }
        }

        // Orden de inserción exigido por las políticas RLS (ver Modulo3RlsPoliticas): vehiculo →
        // cliente_vehiculo → vehiculo_placa. La política INSERT de vehiculo_placa exige que el
        // cliente_vehiculo de esta misma operación ya exista; agregarlo antes a la sesión de EF
        // Core no alcanza, el orden real lo decide el orden en que SaveChanges emite los INSERT.
        var clienteVehiculo = new ClienteVehiculo(empresaId, request.ClienteId, vehiculo.Id, request.Rol);
        db.ClienteVehiculos.Add(clienteVehiculo);

        if (esNuevo && placa is not null)
        {
            db.VehiculoPlacas.Add(new VehiculoPlaca(vehiculo.Id, placa, DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime)));
        }

        await db.SaveChangesAsync(cancellationToken);

        // No bloquea (ver RegistrarVehiculoCommandValidator): se informa igual, para que quien
        // llama pueda avisar que ese VIN quedó guardado tal cual se recibió, sin que su dígito
        // verificador cierre.
        var advertenciaVin = vin is not null && !VinValidator.EsValido(vin)
            ? "El VIN no tiene un dígito verificador válido; se guardó igual."
            : null;

        return new RegistrarVehiculoResultado(vehiculo.Id, clienteVehiculo.Id, esNuevo, advertenciaVin);
    }

    // IgnoreQueryFilters en ambos DbSet cuando global=true: Vehiculo y VehiculoPlaca comparten el
    // mismo filtro (ver AppDbContext), así que sin esto el JOIN seguiría acotado a los vínculos de
    // esta empresa aunque la sesión ya esté elevada para RLS.
    private static Task<Vehiculo?> BuscarPorPlacaActivaAsync(IAppDbContext db, string placa, bool global, CancellationToken cancellationToken)
    {
        var vehiculos = global ? db.Vehiculos.IgnoreQueryFilters() : db.Vehiculos;
        var placas = global ? db.VehiculoPlacas.IgnoreQueryFilters() : db.VehiculoPlacas;

        return (from v in vehiculos
                join p in placas on v.Id equals p.VehiculoId
                where p.Placa == placa && p.Hasta == null
                select v).FirstOrDefaultAsync(cancellationToken);
    }
}
