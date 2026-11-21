using Microsoft.EntityFrameworkCore;
using TallerOS.Application.Comun;
using TallerOS.Application.Organizacion.Empresas.Commands.CrearEmpresa;
using TallerOS.Infrastructure.Persistence;
using TallerOS.Infrastructure.Seguridad;

namespace TallerOS.Api.Tests.Integracion;

/// <summary>Prueba de integración contra Postgres real (mismo motivo que AislamientoEntreEmpresasTests).
/// Cubre la condición que puso el usuario al aprobar el Backend del Módulo 4: los 8 estados
/// estándar y el contador de numeración deben nacer con cualquier empresa NUEVA (no solo con la
/// demo que siembra HasData), y volver a llamar la siembra para la misma empresa no debe duplicar
/// filas (ver EstadosOtEstandar.SembrarAsync, llamada desde CrearEmpresaCommandHandler).</summary>
public sealed class EstadosOtEstandarTests
{
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

    // Plan "Básico", sembrado por PlanConfiguration.HasData.
    private static readonly Guid PlanBasicoId = Guid.Parse("916d3c96-5570-4817-9dcb-b21060472d9b");

    [Fact]
    public async Task Crear_empresa_nueva_siembra_8_estados_y_1_contador_sin_duplicar_al_repetir_la_siembra()
    {
        // El propio CrearEmpresaCommandHandler se eleva a sí mismo (ElevarPrivilegiosSinEmpresaAsync)
        // para poder comprobar RUC/email duplicados antes de que la empresa exista; el contexto de
        // afuera solo necesita EsSuperAdmin=true para que el filtro global de EF no le oculte Planes,
        // Usuarios ni Permisos durante esas comprobaciones.
        var superAdmin = new TenantContextFijo(empresaId: null, esSuperAdmin: true);
        var db = CrearContexto(superAdmin);
        Guid empresaId;
        try
        {
            var rucDePrueba = $"999{Random.Shared.Next(0, 1_000_000_000):D10}";
            var email = $"dueño.prueba.{Guid.NewGuid():N}@talleros-test.local";
            var handler = new CrearEmpresaCommandHandler(db, new HashService());

            var resultado = await handler.Handle(
                new CrearEmpresaCommand("Empresa de prueba (siembra de estados)", rucDePrueba, PlanBasicoId, email, "Clave123!"),
                CancellationToken.None);
            empresaId = resultado.EmpresaId;

            // IgnoreQueryFilters: este contexto ya es super admin, pero así la prueba no depende de
            // esa elevación para "ver" las filas, igual que el resto de pruebas de este módulo.
            var estados = await db.EstadosOt.IgnoreQueryFilters()
                .Where(e => e.EmpresaId == empresaId).OrderBy(e => e.Orden).ToListAsync(CancellationToken.None);
            Assert.Equal(8, estados.Count);
            Assert.Equal(Enumerable.Range(1, 8), estados.Select(e => (int)e.Orden));
            Assert.True(estados.Last().EsFinal);
            Assert.All(estados.SkipLast(1), e => Assert.False(e.EsFinal));

            var contador = await db.ContadoresNumeroOt.IgnoreQueryFilters()
                .SingleAsync(c => c.EmpresaId == empresaId, CancellationToken.None);
            Assert.Equal(0, contador.UltimoNumero);

            // Repetir la siembra para la misma empresa (ej. un reintento del cliente ante un timeout
            // de red) no debe duplicar las 8 filas: SembrarAsync encuentra las que ya existen y no
            // agrega nada más. No hace falta llamar SaveChangesAsync: si no agrega nada, no hay nada
            // que guardar.
            await TallerOS.Application.OrdenesTrabajo.EstadosOtEstandar.SembrarAsync(db, empresaId, CancellationToken.None);
            await db.SaveChangesAsync(CancellationToken.None);

            await using var verificacion = CrearContexto(superAdmin);
            var totalTrasRepetir = await verificacion.EstadosOt.IgnoreQueryFilters()
                .CountAsync(e => e.EmpresaId == empresaId, CancellationToken.None);
            Assert.Equal(8, totalTrasRepetir);
            var contadoresTrasRepetir = await verificacion.ContadoresNumeroOt.IgnoreQueryFilters()
                .CountAsync(c => c.EmpresaId == empresaId, CancellationToken.None);
            Assert.Equal(1, contadoresTrasRepetir);
        }
        finally
        {
            await LimpiarEmpresaDePruebaAsync(db);
            await db.DisposeAsync();
        }
    }

    /// <summary>Borra, en orden de dependencia, lo que esta prueba haya alcanzado a crear. Busca por
    /// nombre en vez de recibir el id: si Handle lanzó antes de que el try lo asignara, esta limpieza
    /// corre igual (y simplemente no encuentra nada).</summary>
    private static async Task LimpiarEmpresaDePruebaAsync(AppDbContext db)
    {
        var empresas = await db.Empresas.IgnoreQueryFilters()
            .Where(e => e.Nombre == "Empresa de prueba (siembra de estados)").ToListAsync(CancellationToken.None);
        foreach (var empresa in empresas)
        {
            var id = empresa.Id;
            db.ContadoresNumeroOt.RemoveRange(await db.ContadoresNumeroOt.IgnoreQueryFilters().Where(c => c.EmpresaId == id).ToListAsync(CancellationToken.None));
            db.EstadosOt.RemoveRange(await db.EstadosOt.IgnoreQueryFilters().Where(e => e.EmpresaId == id).ToListAsync(CancellationToken.None));
            db.UsuarioRoles.RemoveRange(await db.UsuarioRoles.IgnoreQueryFilters().Where(ur => ur.EmpresaId == id).ToListAsync(CancellationToken.None));
            db.RolPermisos.RemoveRange(await db.RolPermisos.IgnoreQueryFilters().Where(rp => db.Roles.IgnoreQueryFilters().Any(r => r.Id == rp.RolId && r.EmpresaId == id)).ToListAsync(CancellationToken.None));
            db.Roles.RemoveRange(await db.Roles.IgnoreQueryFilters().Where(r => r.EmpresaId == id).ToListAsync(CancellationToken.None));
            db.Suscripciones.RemoveRange(await db.Suscripciones.IgnoreQueryFilters().Where(s => s.EmpresaId == id).ToListAsync(CancellationToken.None));
            await db.SaveChangesAsync(CancellationToken.None);

            var usuarios = await db.Usuarios.Where(u => u.Email.EndsWith("@talleros-test.local")).ToListAsync(CancellationToken.None);
            db.Usuarios.RemoveRange(usuarios);
            await db.SaveChangesAsync(CancellationToken.None);

            db.Empresas.Remove(empresa);
            await db.SaveChangesAsync(CancellationToken.None);
        }
    }
}
