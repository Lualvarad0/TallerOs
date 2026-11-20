using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TallerOS.Application.Catalogos;
using TallerOS.Application.Comun;
using TallerOS.Infrastructure.Persistence;
using TallerOS.Infrastructure.Seguridad;

namespace TallerOS.Infrastructure;

public static class DependencyInjection
{
    /// <summary>connectionStringName: "App" para la API (rol sin DDL ni BYPASSRLS), "Migrator" solo
    /// para el proceso que corre `dotnet ef database update`. Nunca usar "Migrator" en la API.</summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration, string connectionStringName = "App")
    {
        var connectionString = configuration.GetConnectionString(connectionStringName)
            ?? throw new InvalidOperationException($"Falta ConnectionStrings:{connectionStringName} en la configuración.");

        services.AddScoped<ITenantContext, TenantContextPorDefecto>();
        services.AddScoped<TenantSessionInterceptor>();
        services.AddSingleton<IHashService, HashService>();

        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.AddSingleton<ITokenService, TokenService>();

        services.AddDbContext<AppDbContext>((sp, options) =>
            options
                .UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(sp.GetRequiredService<TenantSessionInterceptor>()));

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<ICatalogoSimilitudService, CatalogoSimilitudService>();

        return services;
    }
}
