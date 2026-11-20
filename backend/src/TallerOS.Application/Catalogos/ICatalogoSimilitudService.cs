namespace TallerOS.Application.Catalogos;

/// <summary>Nombres de marca/modelo parecidos a uno nuevo (trigramas de Postgres, pg_trgm): detecta
/// casi-duplicados que el índice único no atrapa (ej. "Toyota" vs "Toyotaa"). Implementado en
/// Infrastructure porque similarity() es específico de Postgres; Application solo conoce el
/// contrato, igual que ITokenService/IHashService.</summary>
public interface ICatalogoSimilitudService
{
    Task<IReadOnlyList<string>> BuscarMarcasParecidasAsync(short tipoVehiculoId, string nombre, CancellationToken cancellationToken);

    Task<IReadOnlyList<string>> BuscarModelosParecidosAsync(Guid marcaId, string nombre, CancellationToken cancellationToken);
}
