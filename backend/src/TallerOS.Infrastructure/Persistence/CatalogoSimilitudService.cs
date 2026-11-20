using Microsoft.EntityFrameworkCore;
using TallerOS.Application.Catalogos;

namespace TallerOS.Infrastructure.Persistence;

public sealed class CatalogoSimilitudService(AppDbContext db) : ICatalogoSimilitudService
{
    // Por debajo, nombres genuinamente distintos (ej. "Kia" vs "Kya", cortos); por encima, lo más
    // probable es el mismo nombre mal escrito (ej. "Toyota" vs "Toyotaa").
    private const double UmbralSimilitud = 0.6;

    public async Task<IReadOnlyList<string>> BuscarMarcasParecidasAsync(
        short tipoVehiculoId, string nombre, CancellationToken cancellationToken) =>
        await db.Database.SqlQueryRaw<string>(
            """
            select nombre from marca
            where tipo_vehiculo_id = {0} and similarity(nombre, {1}) >= {2}
            order by similarity(nombre, {1}) desc
            limit 3
            """,
            tipoVehiculoId, nombre, UmbralSimilitud).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<string>> BuscarModelosParecidosAsync(
        Guid marcaId, string nombre, CancellationToken cancellationToken) =>
        await db.Database.SqlQueryRaw<string>(
            """
            select nombre from modelo
            where marca_id = {0} and similarity(nombre, {1}) >= {2}
            order by similarity(nombre, {1}) desc
            limit 3
            """,
            marcaId, nombre, UmbralSimilitud).ToListAsync(cancellationToken);
}
