using Microsoft.EntityFrameworkCore;
using TallerOS.Application.Comun;
using TallerOS.Application.Comun.Cqrs;
using TallerOS.Application.Comun.Excepciones;

namespace TallerOS.Application.Organizacion.Empresas.Commands.CambiarEstadoEmpresa;

public sealed class CambiarEstadoEmpresaCommandHandler(IAppDbContext db) : IRequestHandler<CambiarEstadoEmpresaCommand, Unit>
{
    public async Task<Unit> Handle(CambiarEstadoEmpresaCommand request, CancellationToken cancellationToken)
    {
        var empresa = await db.Empresas.FirstOrDefaultAsync(e => e.Id == request.EmpresaId, cancellationToken)
            ?? throw new EntidadNoEncontradaException("Empresa", request.EmpresaId);

        empresa.CambiarEstado(request.NuevoEstado);
        await db.SaveChangesAsync(cancellationToken);

        return Unit.Valor;
    }
}
