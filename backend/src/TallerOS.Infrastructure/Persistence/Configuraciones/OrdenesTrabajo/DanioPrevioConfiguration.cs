using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TallerOS.Domain.OrdenesTrabajo;

namespace TallerOS.Infrastructure.Persistence.Configuraciones.OrdenesTrabajo;

public class DanioPrevioConfiguration : IEntityTypeConfiguration<DanioPrevio>
{
    public void Configure(EntityTypeBuilder<DanioPrevio> builder)
    {
        builder.ToTable("danio_previo");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Zona).HasMaxLength(40).IsRequired();
        builder.Property(d => d.Tipo).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(d => d.Descripcion).HasColumnType("text");
        builder.Property(d => d.FotoUrl).HasColumnType("text");

        builder.HasOne<Recepcion>().WithMany()
            .HasForeignKey(d => new { d.EmpresaId, d.RecepcionId })
            .HasPrincipalKey(r => new { r.EmpresaId, r.Id })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(d => new { d.EmpresaId, d.RecepcionId });
    }
}
