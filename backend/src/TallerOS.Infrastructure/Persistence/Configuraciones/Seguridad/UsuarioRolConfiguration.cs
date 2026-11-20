using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TallerOS.Domain.Organizacion;
using TallerOS.Domain.Seguridad;

namespace TallerOS.Infrastructure.Persistence.Configuraciones.Seguridad;

public class UsuarioRolConfiguration : IEntityTypeConfiguration<UsuarioRol>
{
    public void Configure(EntityTypeBuilder<UsuarioRol> builder)
    {
        builder.ToTable("usuario_rol");
        builder.HasKey(ur => ur.Id);

        builder.HasOne<Usuario>().WithMany().HasForeignKey(ur => ur.UsuarioId).OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Rol>()
            .WithMany()
            .HasForeignKey(ur => new { ur.EmpresaId, ur.RolId })
            .HasPrincipalKey(r => new { r.EmpresaId, r.Id })
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Sucursal>()
            .WithMany()
            .HasForeignKey(ur => new { ur.EmpresaId, ur.SucursalId })
            .HasPrincipalKey(s => new { s.EmpresaId, s.Id })
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        // usuario_id primero: antes de iniciar sesión no se sabe la empresa, así que la primera
        // consulta ("¿a qué empresas tiene acceso este usuario?") busca solo por usuario_id.
        builder.HasIndex(ur => ur.UsuarioId);
        builder.HasIndex(ur => new { ur.EmpresaId, ur.UsuarioId });

        // Sin duplicar la misma asignación. sucursal_id nulo ("todas las sucursales") y un valor
        // concreto no pueden chocar con un índice único normal porque NULL nunca es igual a NULL,
        // así que son dos índices parciales según el caso.
        builder.HasIndex(ur => new { ur.UsuarioId, ur.EmpresaId, ur.RolId })
            .IsUnique()
            .HasFilter("sucursal_id IS NULL")
            .HasDatabaseName("ux_usuario_rol_todas_sucursales");

        builder.HasIndex(ur => new { ur.UsuarioId, ur.EmpresaId, ur.RolId, ur.SucursalId })
            .IsUnique()
            .HasFilter("sucursal_id IS NOT NULL")
            .HasDatabaseName("ux_usuario_rol_una_sucursal");

        // El usuario demo es Dueño/Gerente de toda la empresa (sucursal_id nulo = todas).
        builder.HasData(new
        {
            Id = Guid.Parse("47a1a500-9a46-4bf0-a2ed-62f8f8947158"),
            UsuarioId = Guid.Parse("ce894d4e-9e31-492c-8230-8dba05250a94"),
            EmpresaId = Guid.Parse("e598ae6b-3823-446a-b9b3-3229deb3d134"),
            SucursalId = (Guid?)null,
            RolId = Guid.Parse("29d9f9bd-6850-4547-8bfc-7a809c61534e"),
        });
    }
}
