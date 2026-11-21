using Microsoft.EntityFrameworkCore;
using TallerOS.Application.Comun;
using TallerOS.Application.Comun.Cqrs;
using TallerOS.Application.Comun.Excepciones;
using TallerOS.Domain.OrdenesTrabajo;

namespace TallerOS.Application.OrdenesTrabajo.Commands.RegistrarRecepcion;

public sealed class RegistrarRecepcionCommandHandler(IAppDbContext db, ITenantContext tenant)
    : IRequestHandler<RegistrarRecepcionCommand, RegistrarRecepcionResultado>
{
    public async Task<RegistrarRecepcionResultado> Handle(RegistrarRecepcionCommand request, CancellationToken cancellationToken)
    {
        var empresaId = tenant.EmpresaId ?? throw new SinContextoDeEmpresaException();

        // El vínculo cliente-vehículo es lo que RegistrarVehiculo (Módulo 3) crea; sin él, esta
        // empresa nunca vio este vehículo o nunca lo asoció a este cliente.
        var vinculoExiste = await db.ClienteVehiculos.AnyAsync(
            cv => cv.EmpresaId == empresaId && cv.ClienteId == request.ClienteId && cv.VehiculoId == request.VehiculoId,
            cancellationToken);
        if (!vinculoExiste)
        {
            throw new ConflictoDeNegocioException("El cliente y el vehículo no están vinculados en esta empresa.");
        }

        // El de menor "orden": siempre existe porque EstadosOtEstandar los siembra al crear la
        // empresa (ver CrearEmpresaCommandHandler); si no existiera sería un bug real, no un 404.
        var estadoInicial = await db.EstadosOt.OrderBy(e => e.Orden).FirstAsync(cancellationToken);

        var numero = await db.SiguienteNumeroOtAsync(empresaId, cancellationToken);

        var ot = new OrdenTrabajo(
            empresaId, request.SucursalId, numero, request.VehiculoId, request.ClienteId,
            estadoInicial.Id, request.Prioridad, request.KmIngreso, request.AsesorPersonalId);
        db.OrdenesTrabajo.Add(ot);

        var recepcion = new Recepcion(empresaId, ot.Id, request.Combustible, request.Objetos, request.Observaciones);
        db.Recepciones.Add(recepcion);

        db.DaniosPrevios.AddRange(request.DaniosPrevios.Select(d =>
            new DanioPrevio(empresaId, recepcion.Id, d.Zona, d.Tipo, d.Descripcion, d.FotoUrl)));

        // de_estado_id nulo: es la primera fila del historial de esta OT, no viene "de" ningún otro.
        db.HistorialEstadoOt.Add(new HistorialEstadoOt(empresaId, ot.Id, deEstadoId: null, estadoInicial.Id, request.UsuarioId));

        await db.SaveChangesAsync(cancellationToken);

        return new RegistrarRecepcionResultado(ot.Id, numero, estadoInicial.Id);
    }
}
