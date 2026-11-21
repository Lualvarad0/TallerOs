using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using TallerOS.Application.Comun;
using TallerOS.Domain.Catalogos;
using TallerOS.Domain.Clientes;
using TallerOS.Domain.Organizacion;
using TallerOS.Domain.OrdenesTrabajo;
using TallerOS.Domain.Plataforma;
using TallerOS.Domain.Seguridad;
using TallerOS.Domain.Vehiculos;

namespace TallerOS.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options, ITenantContext tenantContext) : DbContext(options), IAppDbContext
{
    // Usadas desde las expresiones de HasQueryFilter de abajo (EF las traduce a parámetros SQL).
    private Guid? EmpresaIdActual => tenantContext.EmpresaId;
    private bool EsSuperAdmin => tenantContext.EsSuperAdmin;
    public DbSet<Plan> Planes => Set<Plan>();
    public DbSet<Suscripcion> Suscripciones => Set<Suscripcion>();

    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<Sucursal> Sucursales => Set<Sucursal>();
    public DbSet<Area> Areas => Set<Area>();
    public DbSet<Personal> Personal => Set<Personal>();

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Permiso> Permisos => Set<Permiso>();
    public DbSet<RolPermiso> RolPermisos => Set<RolPermiso>();
    public DbSet<UsuarioRol> UsuarioRoles => Set<UsuarioRol>();
    public DbSet<DispositivoTablet> DispositivosTablet => Set<DispositivoTablet>();

    // Catálogo global (sin empresa_id): ver docs/convenciones.md § Seguridad. Sin HasQueryFilter, igual que
    // Permisos y Planes.
    public DbSet<TipoVehiculo> TiposVehiculo => Set<TipoVehiculo>();
    public DbSet<Marca> Marcas => Set<Marca>();
    public DbSet<Modelo> Modelos => Set<Modelo>();

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<CuentaCliente> CuentasCliente => Set<CuentaCliente>();
    public DbSet<Vehiculo> Vehiculos => Set<Vehiculo>();
    public DbSet<VehiculoPlaca> VehiculoPlacas => Set<VehiculoPlaca>();
    public DbSet<VehiculoPropietario> VehiculoPropietarios => Set<VehiculoPropietario>();
    public DbSet<ClienteVehiculo> ClienteVehiculos => Set<ClienteVehiculo>();

    public DbSet<EstadoOt> EstadosOt => Set<EstadoOt>();
    public DbSet<ContadorNumeroOt> ContadoresNumeroOt => Set<ContadorNumeroOt>();
    public DbSet<OrdenTrabajo> OrdenesTrabajo => Set<OrdenTrabajo>();
    public DbSet<HistorialEstadoOt> HistorialEstadoOt => Set<HistorialEstadoOt>();
    public DbSet<Recepcion> Recepciones => Set<Recepcion>();
    public DbSet<DanioPrevio> DaniosPrevios => Set<DanioPrevio>();
    public DbSet<TipoTrabajo> TiposTrabajo => Set<TipoTrabajo>();
    public DbSet<Subtarea> Subtareas => Set<Subtarea>();
    public DbSet<Adjunto> Adjuntos => Set<Adjunto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Primera barrera de aislamiento (la segunda son las políticas RLS de las migraciones): si
        // un handler se olvida de acotar una consulta a su empresa, esto ya la acota por él. Super
        // admin la salta a propósito (necesita ver todas las empresas).
        modelBuilder.Entity<Empresa>().HasQueryFilter(e => EsSuperAdmin || e.Id == EmpresaIdActual);
        modelBuilder.Entity<Sucursal>().HasQueryFilter(s => EsSuperAdmin || s.EmpresaId == EmpresaIdActual);
        modelBuilder.Entity<Area>().HasQueryFilter(a => EsSuperAdmin || a.EmpresaId == EmpresaIdActual);
        modelBuilder.Entity<Personal>().HasQueryFilter(p => EsSuperAdmin || p.EmpresaId == EmpresaIdActual);
        modelBuilder.Entity<Rol>().HasQueryFilter(r => EsSuperAdmin || r.EmpresaId == EmpresaIdActual);
        modelBuilder.Entity<UsuarioRol>().HasQueryFilter(ur => EsSuperAdmin || ur.EmpresaId == EmpresaIdActual);
        modelBuilder.Entity<DispositivoTablet>().HasQueryFilter(d => EsSuperAdmin || d.EmpresaId == EmpresaIdActual);
        modelBuilder.Entity<Suscripcion>().HasQueryFilter(s => EsSuperAdmin || s.EmpresaId == EmpresaIdActual);

        // rol_permiso no tiene empresa_id propio: el filtro llega por su rol_id (subconsulta
        // contra rol), igual que la política RLS de esta tabla.
        modelBuilder.Entity<RolPermiso>().HasQueryFilter(rp =>
            EsSuperAdmin || Roles.Any(r => r.Id == rp.RolId && r.EmpresaId == EmpresaIdActual));

        modelBuilder.Entity<Cliente>().HasQueryFilter(c => EsSuperAdmin || c.EmpresaId == EmpresaIdActual);
        modelBuilder.Entity<ClienteVehiculo>().HasQueryFilter(cv => EsSuperAdmin || cv.EmpresaId == EmpresaIdActual);

        // vehiculo y vehiculo_placa no tienen empresa_id propio (catálogo global, ver docs/convenciones.md):
        // una empresa los ve solo si tiene un ClienteVehiculo vigente a ese vehículo. Crear ese
        // vínculo es lo único que concede acceso (ver ClienteVehiculo); por eso vincular pasa
        // siempre por RegistrarVehiculo, nunca por un endpoint que reciba un VehiculoId suelto.
        modelBuilder.Entity<Vehiculo>().HasQueryFilter(v =>
            EsSuperAdmin || ClienteVehiculos.Any(cv => cv.VehiculoId == v.Id && cv.EmpresaId == EmpresaIdActual));
        modelBuilder.Entity<VehiculoPlaca>().HasQueryFilter(vp =>
            EsSuperAdmin || ClienteVehiculos.Any(cv => cv.VehiculoId == vp.VehiculoId && cv.EmpresaId == EmpresaIdActual));

        // vehiculo_propietario es de Mi Garage, no del taller: ninguna empresa lo ve por tener un
        // ClienteVehiculo (eso filtraría el dato de propiedad real de la cuenta del cliente a
        // cualquier taller que alguna vez atendió el vehículo). Reservada hasta que se construya la
        // transferencia de propiedad, que define su propia regla de acceso.
        modelBuilder.Entity<VehiculoPropietario>().HasQueryFilter(vp => EsSuperAdmin);

        // cuenta_cliente solo es visible si esta empresa vinculó directamente a su cliente (no por
        // haber atendido alguna vez un vehículo que esa cuenta también posee).
        modelBuilder.Entity<CuentaCliente>().HasQueryFilter(cc =>
            EsSuperAdmin || Clientes.Any(c => c.CuentaClienteId == cc.Id && c.EmpresaId == EmpresaIdActual));

        // Módulo 4: todas estas tablas tienen empresa_id propio (regla número uno de Seguridad en
        // docs/convenciones.md), así que el filtro es siempre directo, sin EXISTS.
        modelBuilder.Entity<EstadoOt>().HasQueryFilter(e => EsSuperAdmin || e.EmpresaId == EmpresaIdActual);
        modelBuilder.Entity<ContadorNumeroOt>().HasQueryFilter(c => EsSuperAdmin || c.EmpresaId == EmpresaIdActual);
        modelBuilder.Entity<OrdenTrabajo>().HasQueryFilter(o => EsSuperAdmin || o.EmpresaId == EmpresaIdActual);
        modelBuilder.Entity<HistorialEstadoOt>().HasQueryFilter(h => EsSuperAdmin || h.EmpresaId == EmpresaIdActual);
        modelBuilder.Entity<Recepcion>().HasQueryFilter(r => EsSuperAdmin || r.EmpresaId == EmpresaIdActual);
        modelBuilder.Entity<DanioPrevio>().HasQueryFilter(d => EsSuperAdmin || d.EmpresaId == EmpresaIdActual);
        modelBuilder.Entity<TipoTrabajo>().HasQueryFilter(t => EsSuperAdmin || t.EmpresaId == EmpresaIdActual);
        modelBuilder.Entity<Subtarea>().HasQueryFilter(s => EsSuperAdmin || s.EmpresaId == EmpresaIdActual);
        modelBuilder.Entity<Adjunto>().HasQueryFilter(a => EsSuperAdmin || a.EmpresaId == EmpresaIdActual);
    }

    public async Task<int> SiguienteNumeroOtAsync(Guid empresaId, CancellationToken cancellationToken)
    {
        // Database.SqlQuery siempre envuelve el SQL que recibe en un SELECT adicional (para poder
        // componer Where/Select/etc. encima), y un UPDATE...RETURNING no es válido ahí dentro, ni
        // siquiera metiéndolo en su propio WITH ("WITH clause containing a data-modifying statement
        // must be at the top level" — un WITH con UPDATE no puede anidarse en el FROM de otro
        // SELECT). La única forma de ejecutar este UPDATE atómico y leer su RETURNING es con
        // ADO.NET directo, sobre la misma conexión (y transacción ambiente, si hay una) que ya usa
        // este DbContext.
        await Database.OpenConnectionAsync(cancellationToken);
        var conexion = (NpgsqlConnection)Database.GetDbConnection();
        await using var comando = conexion.CreateCommand();
        comando.CommandText = "UPDATE contador_numero_ot SET ultimo_numero = ultimo_numero + 1 WHERE empresa_id = @empresaId RETURNING ultimo_numero";
        comando.Parameters.AddWithValue("empresaId", empresaId);
        comando.Transaction = (NpgsqlTransaction?)Database.CurrentTransaction?.GetDbTransaction();

        var resultado = await comando.ExecuteScalarAsync(cancellationToken);
        return (int)resultado!;
    }

    public async Task ElevarPrivilegiosSinEmpresaAsync(CancellationToken cancellationToken)
    {
        // OpenConnectionAsync primero: sin esto, ExecuteSqlRawAsync abre y cierra su propia conexión
        // (TenantSessionInterceptor la vuelve a fijar en el valor sin privilegios al abrirla de nuevo
        // para la siguiente consulta/SaveChanges), y la elevación se pierde antes de usarse.
        await Database.OpenConnectionAsync(cancellationToken);
        await Database.ExecuteSqlRawAsync("SELECT set_config('app.es_super_admin', 'true', false);", cancellationToken);
    }
}
