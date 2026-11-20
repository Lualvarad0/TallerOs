using Microsoft.EntityFrameworkCore;
using TallerOS.Application.Comun;
using TallerOS.Application.Comun.Cqrs;
using TallerOS.Application.Comun.Excepciones;

namespace TallerOS.Application.Organizacion.Empresas.Queries.ObtenerEmpresa;

public sealed class ObtenerEmpresaQueryHandler(IAppDbContext db) : IRequestHandler<ObtenerEmpresaQuery, EmpresaDto>
{
    public async Task<EmpresaDto> Handle(ObtenerEmpresaQuery request, CancellationToken cancellationToken)
    {
        return await db.Empresas
            .Where(e => e.Id == request.EmpresaId)
            .Select(e => new EmpresaDto(e.Id, e.Nombre, e.Ruc, e.Estado.ToString(), e.CreadoEn))
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new EntidadNoEncontradaException("Empresa", request.EmpresaId);
    }
}
