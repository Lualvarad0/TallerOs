using Microsoft.EntityFrameworkCore;
using TallerOS.Application.Comun;
using TallerOS.Application.Comun.Cqrs;
using TallerOS.Application.Comun.Excepciones;

namespace TallerOS.Application.Clientes.Commands.DesactivarCliente;

public sealed class DesactivarClienteCommandHandler(IAppDbContext db) : IRequestHandler<DesactivarClienteCommand, Unit>
{
    public async Task<Unit> Handle(DesactivarClienteCommand request, CancellationToken cancellationToken)
    {
        var cliente = await db.Clientes.FirstOrDefaultAsync(c => c.Id == request.ClienteId && c.EliminadoEn == null, cancellationToken)
            ?? throw new EntidadNoEncontradaException("Cliente", request.ClienteId);

        cliente.Desactivar();
        await db.SaveChangesAsync(cancellationToken);

        return Unit.Valor;
    }
}
