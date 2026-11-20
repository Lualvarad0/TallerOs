using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TallerOS.Domain.Organizacion;
using TallerOS.Domain.Seguridad;

namespace TallerOS.Infrastructure.Persistence.Configuraciones.Seguridad;

public class RolConfiguration : IEntityTypeConfiguration<Rol>
{
    public void Configure(EntityTypeBuilder<Rol> builder)
    {
        builder.ToTable("rol");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Nombre).HasMaxLength(80).IsRequired();

        builder.HasOne<Empresa>().WithMany().HasForeignKey(r => r.EmpresaId).OnDelete(DeleteBehavior.Restrict);

        // Llave alterna para la FK compuesta desde UsuarioRol.
        builder.HasAlternateKey(r => new { r.EmpresaId, r.Id });

        builder.HasIndex(r => r.EmpresaId);
        builder.HasIndex(r => new { r.EmpresaId, r.Nombre }).IsUnique().HasFilter("eliminado_en IS NULL");

        builder.HasData(
            new
            {
                Id = Guid.Parse("29d9f9bd-6850-4547-8bfc-7a809c61534e"),
                EmpresaId = Guid.Parse("e598ae6b-3823-446a-b9b3-3229deb3d134"),
                Nombre = "Dueño/Gerente",
            },
            new
            {
                Id = Guid.Parse("2b3ac12b-47be-4e9d-ab20-0f3a12598fa0"),
                EmpresaId = Guid.Parse("e598ae6b-3823-446a-b9b3-3229deb3d134"),
                Nombre = "Jefe de sucursal",
            });
    }
}
