using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TallerOS.Domain.Seguridad;

namespace TallerOS.Infrastructure.Persistence.Configuraciones.Seguridad;

public class RolPermisoConfiguration : IEntityTypeConfiguration<RolPermiso>
{
    public void Configure(EntityTypeBuilder<RolPermiso> builder)
    {
        builder.ToTable("rol_permiso");
        builder.HasKey(rp => new { rp.RolId, rp.PermisoCodigo });

        // No lleva empresa_id propio: el aislamiento llega por rol_id -> rol.empresa_id (RLS aquí
        // usa un EXISTS contra "rol" en vez de comparar una columna local).
        builder.HasOne<Rol>().WithMany().HasForeignKey(rp => rp.RolId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Permiso>().WithMany().HasForeignKey(rp => rp.PermisoCodigo).OnDelete(DeleteBehavior.Cascade);

        var dueno = Guid.Parse("29d9f9bd-6850-4547-8bfc-7a809c61534e");
        var jefeDeSucursal = Guid.Parse("2b3ac12b-47be-4e9d-ab20-0f3a12598fa0");

        builder.HasData(
            // Dueño/Gerente: todos los permisos del módulo.
            new { RolId = dueno, PermisoCodigo = "empresa.configurar" },
            new { RolId = dueno, PermisoCodigo = "sucursal.gestionar" },
            new { RolId = dueno, PermisoCodigo = "area.gestionar" },
            new { RolId = dueno, PermisoCodigo = "personal.gestionar" },
            new { RolId = dueno, PermisoCodigo = "rol.gestionar" },
            new { RolId = dueno, PermisoCodigo = "usuario.gestionar" },
            new { RolId = dueno, PermisoCodigo = "usuario_rol.asignar" },
            new { RolId = dueno, PermisoCodigo = "dispositivo_tablet.gestionar" },
            // Jefe de sucursal: solo lo operativo de su sucursal, no la empresa ni los roles.
            new { RolId = jefeDeSucursal, PermisoCodigo = "area.gestionar" },
            new { RolId = jefeDeSucursal, PermisoCodigo = "personal.gestionar" },
            new { RolId = jefeDeSucursal, PermisoCodigo = "dispositivo_tablet.gestionar" });
    }
}
