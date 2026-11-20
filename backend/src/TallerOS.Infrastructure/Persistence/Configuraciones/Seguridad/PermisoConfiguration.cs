using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TallerOS.Domain.Seguridad;

namespace TallerOS.Infrastructure.Persistence.Configuraciones.Seguridad;

public class PermisoConfiguration : IEntityTypeConfiguration<Permiso>
{
    public void Configure(EntityTypeBuilder<Permiso> builder)
    {
        builder.ToTable("permiso");

        // El código (ej. "ot.asignar") es la clave: lo usan directamente el backend y el frontend
        // para chequear permisos, así que no tiene sentido un id numérico que solo serviría para el join.
        builder.HasKey(p => p.Codigo);
        builder.Property(p => p.Codigo).HasMaxLength(60);

        builder.Property(p => p.Nombre).HasMaxLength(120).IsRequired();
        builder.Property(p => p.Descripcion).HasColumnType("text");

        // Permisos de Módulo 1 (organización y seguridad) y Módulo 2 (catálogo de vehículos). Los de
        // otros módulos (ej. "ot.asignar") se agregan cuando se construya ese módulo, no antes.
        builder.HasData(
            new { Codigo = "empresa.configurar", Nombre = "Configurar empresa", Descripcion = "Editar los datos y el estado de la empresa" },
            new { Codigo = "sucursal.gestionar", Nombre = "Gestionar sucursales", Descripcion = "Crear, editar y desactivar sucursales" },
            new { Codigo = "area.gestionar", Nombre = "Gestionar áreas", Descripcion = "Crear, editar y desactivar áreas dentro de una sucursal" },
            new { Codigo = "personal.gestionar", Nombre = "Gestionar personal", Descripcion = "Crear, editar y desactivar personal (con o sin acceso)" },
            new { Codigo = "rol.gestionar", Nombre = "Gestionar roles", Descripcion = "Crear roles y asignarles permisos" },
            new { Codigo = "usuario.gestionar", Nombre = "Gestionar usuarios", Descripcion = "Crear cuentas de usuario e invitar personal" },
            new { Codigo = "usuario_rol.asignar", Nombre = "Asignar roles a usuarios", Descripcion = "Dar o quitar un rol a un usuario, por sucursal o para toda la empresa" },
            new { Codigo = "dispositivo_tablet.gestionar", Nombre = "Gestionar tablets", Descripcion = "Registrar o revocar tablets compartidas para fichar con PIN" },
            new { Codigo = "catalogo_vehiculo.gestionar", Nombre = "Gestionar catálogo de vehículos", Descripcion = "Crear marcas y modelos nuevos en el catálogo global cuando falte uno" });
    }
}
