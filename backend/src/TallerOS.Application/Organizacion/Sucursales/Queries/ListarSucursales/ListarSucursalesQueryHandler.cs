using Microsoft.EntityFrameworkCore;
using TallerOS.Application.Comun;
using TallerOS.Application.Comun.Cqrs;

namespace TallerOS.Application.Organizacion.Sucursales.Queries.ListarSucursales;

public sealed class ListarSucursalesQueryHandler(IAppDbContext db) : IRequestHandler<ListarSucursalesQuery, IReadOnlyList<SucursalDto>>
{
    public async Task<IReadOnlyList<SucursalDto>> Handle(ListarSucursalesQuery request, CancellationToken cancellationToken)
    {
        // El filtro global por empresa_id ya acota esto a la empresa de la sesión.
        return await db.Sucursales
            .Where(s => s.EliminadoEn == null)
            .OrderBy(s => s.Nombre)
            .Select(s => new SucursalDto(s.Id, s.Nombre, s.Direccion))
            .ToListAsync(cancellationToken);
    }
}
