using Microsoft.EntityFrameworkCore;
using TallerOS.Application.Comun;
using TallerOS.Domain.Organizacion;
using TallerOS.Infrastructure.Persistence;

namespace TallerOS.Api.Tests.Integracion;

/// <summary>Prueba de integración contra un Postgres real (no InMemory: RLS es una característica
/// de Postgres, no se puede probar sin él). Necesita el Postgres local de infra/docker-compose.yml
/// con las migraciones del Módulo 1 aplicadas, y ConnectionStrings__App en el entorno (igual que la
/// Api; ver .env). No usa talleros_migrator a propósito: la Api nunca corre con ese rol, y esta
/// prueba corre contra el mismo rol y las mismas políticas que la Api en producción.
///
/// Verifica las DOS barreras de aislamiento por separado: el filtro global de EF Core (AppDbContext)
/// y, saltándoselo con IgnoreQueryFilters(), las políticas RLS de Postgres (ver la migración
/// Modulo1RlsPoliticas). Si solo probáramos con el filtro de EF puesto, un bug que lo borre mañana
/// no se notaría en esta prueba.</summary>
public sealed class AislamientoEntreEmpresasTests
{
    // Sembrada por EmpresaConfiguration.HasData justamente para esto.
    private static readonly Guid EmpresaDemoId = Guid.Parse("e598ae6b-3823-446a-b9b3-3229deb3d134");

    private sealed class TenantContextFijo(Guid? empresaId, bool esSuperAdmin) : ITenantContext
    {
        public Guid? EmpresaId { get; } = empresaId;
        public bool EsSuperAdmin { get; } = esSuperAdmin;
    }

    private static AppDbContext CrearContexto(ITenantContext tenant)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__App")
            ?? throw new InvalidOperationException(
                "Esta prueba necesita Postgres local con las migraciones del Módulo 1 aplicadas y " +
                "la variable de entorno ConnectionStrings__App (la misma cadena que usa la Api; ver .env).");

        var opciones = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention()
            .AddInterceptors(new TenantSessionInterceptor(tenant))
            .Options;

        return new AppDbContext(opciones, tenant);
    }

    [Fact]
    public async Task EmpresaA_no_ve_sucursales_de_EmpresaB_ni_con_filtro_de_EF_ni_con_RLS_de_Postgres()
    {
        var superAdmin = new TenantContextFijo(empresaId: null, esSuperAdmin: true);
        var setup = CrearContexto(superAdmin);

        try
        {
            // Sin esto, el INSERT de la empresa B (y de las dos sucursales) chocaría con la misma
            // política RLS que protege las lecturas: WITH CHECK también exige la elevación.
            await setup.ElevarPrivilegiosSinEmpresaAsync(CancellationToken.None);

            var rucDePrueba = $"999{Random.Shared.Next(0, 1_000_000_000):D10}";
            var empresaB = new Empresa("Empresa B (prueba de aislamiento)", rucDePrueba);
            setup.Empresas.Add(empresaB);
            await setup.SaveChangesAsync(CancellationToken.None);

            var sucursalA = new Sucursal(EmpresaDemoId, "Sucursal A (prueba de aislamiento)", direccion: null);
            var sucursalB = new Sucursal(empresaB.Id, "Sucursal B (prueba de aislamiento)", direccion: null);
            setup.Sucursales.AddRange(sucursalA, sucursalB);
            await setup.SaveChangesAsync(CancellationToken.None);

            var tenantA = new TenantContextFijo(EmpresaDemoId, esSuperAdmin: false);
            var contextoA = CrearContexto(tenantA);
            try
            {
                // Primera barrera: el filtro global de EF ya acota a la empresa A.
                var conFiltroDeEf = await contextoA.Sucursales.ToListAsync(CancellationToken.None);
                Assert.All(conFiltroDeEf, s => Assert.Equal(EmpresaDemoId, s.EmpresaId));
                Assert.DoesNotContain(conFiltroDeEf, s => s.Id == sucursalB.Id);

                // Segunda barrera: la misma sesión de empresa A, pero saltándose el filtro de EF a
                // propósito. Si solo existiera esa primera barrera, esto devolvería también la fila
                // de la empresa B; con RLS forzado en Postgres, sigue sin verla.
                var sinFiltroDeEf = await contextoA.Sucursales.IgnoreQueryFilters().ToListAsync(CancellationToken.None);
                Assert.Contains(sinFiltroDeEf, s => s.Id == sucursalA.Id);
                Assert.DoesNotContain(sinFiltroDeEf, s => s.Id == sucursalB.Id);
            }
            finally
            {
                await contextoA.DisposeAsync();
            }

            setup.Sucursales.RemoveRange(sucursalA, sucursalB);
            setup.Empresas.Remove(empresaB);
            await setup.SaveChangesAsync(CancellationToken.None);
        }
        finally
        {
            await setup.DisposeAsync();
        }
    }
}
