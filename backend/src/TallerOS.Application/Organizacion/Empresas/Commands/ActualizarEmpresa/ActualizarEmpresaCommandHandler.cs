using Microsoft.EntityFrameworkCore;
using TallerOS.Application.Comun;
using TallerOS.Application.Comun.Cqrs;
using TallerOS.Application.Comun.Excepciones;

namespace TallerOS.Application.Organizacion.Empresas.Commands.ActualizarEmpresa;

public sealed class ActualizarEmpresaCommandHandler(IAppDbContext db) : IRequestHandler<ActualizarEmpresaCommand, Unit>
{
    public async Task<Unit> Handle(ActualizarEmpresaCommand request, CancellationToken cancellationToken)
    {
        // El filtro global por empresa_id (y RLS detrás) ya evita que esto encuentre la empresa de
        // otro: si EmpresaId no es la de la sesión (y no es super admin), simplemente no aparece.
        var empresa = await db.Empresas.FirstOrDefaultAsync(e => e.Id == request.EmpresaId, cancellationToken)
            ?? throw new EntidadNoEncontradaException("Empresa", request.EmpresaId);

        // No se valida aquí el RUC duplicado contra otras empresas: con RLS activo, esta sesión
        // (la de su propia empresa) no puede ver filas de otras para comparar. El índice único de
        // la base es la fuente de verdad; ExceptionHandlingMiddleware traduce esa violación a 409.
        empresa.ActualizarDatos(request.Nombre, request.Ruc);
        await db.SaveChangesAsync(cancellationToken);

        return Unit.Valor;
    }
}
