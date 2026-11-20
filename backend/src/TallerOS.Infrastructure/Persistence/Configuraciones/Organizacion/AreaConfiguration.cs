using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TallerOS.Domain.Organizacion;

namespace TallerOS.Infrastructure.Persistence.Configuraciones.Organizacion;

public class AreaConfiguration : IEntityTypeConfiguration<Area>
{
    public void Configure(EntityTypeBuilder<Area> builder)
    {
        builder.ToTable("area");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Nombre).HasMaxLength(120).IsRequired();

        builder.HasOne<Sucursal>()
            .WithMany()
            .HasForeignKey(a => new { a.EmpresaId, a.SucursalId })
            .HasPrincipalKey(s => new { s.EmpresaId, s.Id })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasAlternateKey(a => new { a.EmpresaId, a.Id });

        builder.HasIndex(a => new { a.EmpresaId, a.SucursalId });
        builder.HasIndex(a => new { a.SucursalId, a.Nombre }).IsUnique().HasFilter("eliminado_en IS NULL");

        builder.HasData(
            new
            {
                Id = Guid.Parse("12ce199f-0fb5-4b45-a0ec-6f6ad26c2a92"),
                EmpresaId = Guid.Parse("e598ae6b-3823-446a-b9b3-3229deb3d134"),
                SucursalId = Guid.Parse("c648727f-1153-4fd1-b06a-3c1d6c1040cc"),
                Nombre = "Mecánica",
            },
            new
            {
                Id = Guid.Parse("b7ae6d86-0fd3-40d9-bc12-c31f6a37011f"),
                EmpresaId = Guid.Parse("e598ae6b-3823-446a-b9b3-3229deb3d134"),
                SucursalId = Guid.Parse("c648727f-1153-4fd1-b06a-3c1d6c1040cc"),
                Nombre = "Administración",
            });
    }
}
