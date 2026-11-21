using Microsoft.EntityFrameworkCore;
using TallerOS.Application.Comun;
using TallerOS.Application.OrdenesTrabajo.Queries.ListarEstadosOt;
using TallerOS.Domain.Organizacion;
using TallerOS.Domain.OrdenesTrabajo;
using TallerOS.Infrastructure.Persistence;

namespace TallerOS.Api.Tests.Integracion;

/// <summary>Prueba de integración contra Postgres real (mismo motivo que AislamientoEntreEmpresasTests).
/// Cubre GET /api/estados-ot (ListarEstadosOtQueryHandler), que CLAUDE.md exige para todo endpoint
/// nuevo: un usuario de la empresa A no puede leer estados de la empresa B.</summary>
public sealed class EstadosOtAislamientoTests
{
    private static readonly Guid EmpresaAId = Guid.Parse("e598ae6b-3823-446a-b9b3-3229deb3d134");

    private sealed class TenantContextFijo(Guid? empresaId, bool esSuperAdmin) : ITenantContext
    {
        public Guid? EmpresaId { get; } = empresaId;
        public bool EsSuperAdmin { get; } = esSuperAdmin;
    }

    private static AppDbContext CrearContexto(ITenantContext tenant)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__App")
            ?? throw new InvalidOperationException(
                "Esta prueba necesita Postgres local con las migraciones aplicadas y la variable " +
                "de entorno ConnectionStrings__App (la misma cadena que usa la Api; ver .env).");

        var opciones = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention()
            .AddInterceptors(new TenantSessionInterceptor(tenant))
            .Options;

        return new AppDbContext(opciones, tenant);
    }

    [Fact]
    public async Task EmpresaA_no_ve_estados_ot_de_EmpresaB_ni_con_el_handler_ni_con_RLS_de_Postgres()
    {
        var superAdmin = new TenantContextFijo(empresaId: null, esSuperAdmin: true);
        var setup = CrearContexto(superAdmin);

        try
        {
            await setup.ElevarPrivilegiosSinEmpresaAsync(CancellationToken.None);

            var rucDePrueba = $"999{Random.Shared.Next(0, 1_000_000_000):D10}";
            var empresaB = new Empresa("Empresa B (prueba aislamiento estados-ot)", rucDePrueba);
            setup.Empresas.Add(empresaB);
            await setup.SaveChangesAsync(CancellationToken.None);

            var estadoA = new EstadoOt(EmpresaAId, "Estado A (prueba aislamiento)", orden: 50, esFinal: false, colorToken: "sf3");
            var estadoB = new EstadoOt(empresaB.Id, "Estado B (prueba aislamiento)", orden: 50, esFinal: false, colorToken: "sf3");
            setup.EstadosOt.AddRange(estadoA, estadoB);
            await setup.SaveChangesAsync(CancellationToken.None);

            var tenantA = new TenantContextFijo(EmpresaAId, esSuperAdmin: false);
            var contextoA = CrearContexto(tenantA);
            try
            {
                // Primera barrera: el mismo query/handler que expone GET /api/estados-ot.
                var resultado = await new ListarEstadosOtQueryHandler(contextoA).Handle(new ListarEstadosOtQuery(), CancellationToken.None);
                Assert.Contains(resultado, e => e.Id == estadoA.Id);
                Assert.DoesNotContain(resultado, e => e.Id == estadoB.Id);

                // Segunda barrera: la misma sesión de empresa A, saltándose el filtro de EF a propósito.
                var sinFiltroDeEf = await contextoA.EstadosOt.IgnoreQueryFilters().ToListAsync(CancellationToken.None);
                Assert.Contains(sinFiltroDeEf, e => e.Id == estadoA.Id);
                Assert.DoesNotContain(sinFiltroDeEf, e => e.Id == estadoB.Id);
            }
            finally
            {
                await contextoA.DisposeAsync();
            }

            setup.EstadosOt.RemoveRange(estadoA, estadoB);
            setup.Empresas.Remove(empresaB);
            await setup.SaveChangesAsync(CancellationToken.None);
        }
        finally
        {
            await setup.DisposeAsync();
        }
    }
}
