using Microsoft.EntityFrameworkCore;
using TallerOS.Domain.Catalogos;
using TallerOS.Domain.Clientes;
using TallerOS.Domain.Organizacion;
using TallerOS.Domain.Plataforma;
using TallerOS.Domain.Seguridad;
using TallerOS.Domain.Vehiculos;

namespace TallerOS.Application.Comun;

/// <summary>Lo que ve Application de la base: los DbSet para consultar con LINQ y SaveChanges. No
/// un repositorio por entidad (serían 12 interfaces casi idénticas); AppDbContext en Infrastructure
/// la implementa. El filtro global por empresa y RLS siguen poniendo el límite real, esto es solo
/// para no acoplar Application a Npgsql/EF Core en concreto.</summary>
public interface IAppDbContext
{
    DbSet<Plan> Planes { get; }
    DbSet<Suscripcion> Suscripciones { get; }

    DbSet<Empresa> Empresas { get; }
    DbSet<Sucursal> Sucursales { get; }
    DbSet<Area> Areas { get; }
    DbSet<Personal> Personal { get; }

    DbSet<Usuario> Usuarios { get; }
    DbSet<Rol> Roles { get; }
    DbSet<Permiso> Permisos { get; }
    DbSet<RolPermiso> RolPermisos { get; }
    DbSet<UsuarioRol> UsuarioRoles { get; }
    DbSet<DispositivoTablet> DispositivosTablet { get; }

    DbSet<TipoVehiculo> TiposVehiculo { get; }
    DbSet<Marca> Marcas { get; }
    DbSet<Modelo> Modelos { get; }

    DbSet<Cliente> Clientes { get; }
    DbSet<CuentaCliente> CuentasCliente { get; }
    DbSet<Vehiculo> Vehiculos { get; }
    DbSet<VehiculoPlaca> VehiculoPlacas { get; }
    DbSet<VehiculoPropietario> VehiculoPropietarios { get; }
    DbSet<ClienteVehiculo> ClienteVehiculos { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>Para tres operaciones legítimas, y ninguna otra — no es un interruptor general de
    /// "modo administrador":
    /// 1. Dar de alta una empresa nueva (todavía sin empresa de sesión).
    /// 2. Resolver a qué empresas tiene acceso un usuario al iniciar sesión (ver IniciarSesionCommand
    ///    /SeleccionarEmpresaCommand), también sin empresa de sesión.
    /// 3. RegistrarVehiculo, buscando un vehículo por VIN o placa entre TODAS las empresas (no solo
    ///    la propia) cuando esta empresa nunca lo vio antes: a diferencia de las dos anteriores, esta
    ///    SÍ corre con una empresa de sesión real, solo necesita leer más allá de ella un instante.
    ///    Lo único que expone de otra empresa son los datos propios del vehículo (vin, marca,
    ///    modelo, año, motor): Vehiculo no referencia qué cliente o empresa lo atendió, así que no
    ///    hay nada más que filtrar.</summary>
    Task ElevarPrivilegiosSinEmpresaAsync(CancellationToken cancellationToken);
}
