using Microsoft.EntityFrameworkCore;
using TallerOS.Application.Comun;
using TallerOS.Application.Comun.Cqrs;
using TallerOS.Application.Comun.Excepciones;
using TallerOS.Domain.Clientes;

namespace TallerOS.Application.Clientes.Commands.CrearCliente;

public sealed class CrearClienteCommandHandler(IAppDbContext db, ITenantContext tenant) : IRequestHandler<CrearClienteCommand, Guid>
{
    public async Task<Guid> Handle(CrearClienteCommand request, CancellationToken cancellationToken)
    {
        var empresaId = tenant.EmpresaId ?? throw new SinContextoDeEmpresaException();
        var identificacion = request.Identificacion.Trim();

        // Mismo chequeo que CrearSucursalCommandHandler: la base ya tiene el índice único real
        // (empresa_id, identificacion) filtrado a eliminado_en IS NULL, ver ClienteConfiguration;
        // esto solo da un mensaje entendible en el caso normal, no una carrera de verdad.
        var identificacionEnUso = await db.Clientes.AnyAsync(
            c => c.EmpresaId == empresaId && c.Identificacion == identificacion && c.EliminadoEn == null, cancellationToken);
        if (identificacionEnUso)
        {
            throw new ConflictoDeNegocioException($"Ya existe un cliente con la identificación '{identificacion}'.");
        }

        var cliente = new Cliente(empresaId, request.Nombre, request.Tipo, identificacion, request.Telefono, request.Email, request.CanalPreferido);
        db.Clientes.Add(cliente);
        await db.SaveChangesAsync(cancellationToken);

        return cliente.Id;
    }
}
