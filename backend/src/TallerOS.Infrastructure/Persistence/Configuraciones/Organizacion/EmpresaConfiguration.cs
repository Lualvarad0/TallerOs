using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TallerOS.Domain.Organizacion;

namespace TallerOS.Infrastructure.Persistence.Configuraciones.Organizacion;

public class EmpresaConfiguration : IEntityTypeConfiguration<Empresa>
{
    public void Configure(EntityTypeBuilder<Empresa> builder)
    {
        builder.ToTable("empresa");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Nombre).HasMaxLength(160).IsRequired();
        builder.Property(e => e.Ruc).HasMaxLength(13).IsRequired();
        builder.Property(e => e.Estado).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(e => e.CreadoEn).IsRequired();

        builder.HasIndex(e => e.Ruc).IsUnique().HasFilter("eliminado_en IS NULL");

        // Empresa demo: lo que usan los demás HasData de este módulo y, más adelante, las pruebas
        // de aislamiento entre empresas (necesitan al menos una empresa real para comparar contra).
        builder.HasData(new
        {
            Id = Guid.Parse("e598ae6b-3823-446a-b9b3-3229deb3d134"),
            Nombre = "Taller Demo",
            Ruc = "1790012345001",
            Estado = EstadoEmpresa.Activa,
            CreadoEn = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
        });
    }
}
