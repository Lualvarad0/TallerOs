using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TallerOS.Infrastructure.Persistence;

/// <summary>Usado solo por la CLI `dotnet ef` (migraciones), no por la API en tiempo de ejecución.
/// Lee ConnectionStrings__Migrator de las variables de entorno: cargar infra/.env antes de correr
/// `dotnet ef` (ver README de infra), porque ese rol es el único con permiso de DDL.</summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Migrator")
            ?? throw new InvalidOperationException(
                "Falta la variable de entorno ConnectionStrings__Migrator. Cárgala desde infra/.env antes de correr `dotnet ef`.");

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention();

        // El tenant context real no importa aquí: `dotnet ef` solo genera SQL de esquema, nunca
        // ejecuta un query filtrado.
        return new AppDbContext(optionsBuilder.Options, new TenantContextPorDefecto());
    }
}
