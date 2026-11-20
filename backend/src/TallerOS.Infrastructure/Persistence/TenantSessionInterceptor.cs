using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Npgsql;
using TallerOS.Application.Comun;

namespace TallerOS.Infrastructure.Persistence;

/// <summary>Cada vez que se abre una conexión del pool, fija las variables de sesión que leen las
/// políticas RLS (ver infra/postgres y las migraciones de RLS): app.empresa_id y app.es_super_admin.
/// Es la mitad "de aplicación" de la segunda barrera; la otra mitad son las políticas en PostgreSQL.</summary>
public sealed class TenantSessionInterceptor(ITenantContext tenantContext) : DbConnectionInterceptor
{
    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        AplicarVariablesDeSesion((NpgsqlConnection)connection);
    }

    public override async Task ConnectionOpenedAsync(
        DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default)
    {
        await AplicarVariablesDeSesionAsync((NpgsqlConnection)connection, cancellationToken);
    }

    private void AplicarVariablesDeSesion(NpgsqlConnection connection)
    {
        using var comando = CrearComando(connection);
        comando.ExecuteNonQuery();
    }

    private async Task AplicarVariablesDeSesionAsync(NpgsqlConnection connection, CancellationToken cancellationToken)
    {
        await using var comando = CrearComando(connection);
        await comando.ExecuteNonQueryAsync(cancellationToken);
    }

    private NpgsqlCommand CrearComando(NpgsqlConnection connection)
    {
        var comando = connection.CreateCommand();
        comando.CommandText = "SELECT set_config('app.empresa_id', $1, false), set_config('app.es_super_admin', $2, false);";
        comando.Parameters.Add(new NpgsqlParameter { Value = tenantContext.EmpresaId?.ToString() ?? string.Empty });
        comando.Parameters.Add(new NpgsqlParameter { Value = tenantContext.EsSuperAdmin ? "true" : "false" });
        return (NpgsqlCommand)comando;
    }
}
