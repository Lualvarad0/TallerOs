using Microsoft.EntityFrameworkCore;
using TallerOS.Application.Comun;
using TallerOS.Application.Comun.Cqrs;
using TallerOS.Application.Comun.Excepciones;

namespace TallerOS.Application.Clientes.Commands.ActualizarCliente;

public sealed class ActualizarClienteCommandHandler(IAppDbContext db) : IRequestHandler<ActualizarClienteCommand, Unit>
{
    public async Task<Unit> Handle(ActualizarClienteCommand request, CancellationToken cancellationToken)
    {
        var cliente = await db.Clientes.FirstOrDefaultAsync(c => c.Id == request.ClienteId && c.EliminadoEn == null, cancellationToken)
            ?? throw new EntidadNoEncontradaException("Cliente", request.ClienteId);

        cliente.ActualizarDatos(request.Nombre, request.Telefono, request.Email, request.CanalPreferido);
        await db.SaveChangesAsync(cancellationToken);

        return Unit.Valor;
    }
}
