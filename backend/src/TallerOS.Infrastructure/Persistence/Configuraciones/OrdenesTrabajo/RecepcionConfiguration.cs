using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TallerOS.Domain.OrdenesTrabajo;

namespace TallerOS.Infrastructure.Persistence.Configuraciones.OrdenesTrabajo;

public class RecepcionConfiguration : IEntityTypeConfiguration<Recepcion>
{
    public void Configure(EntityTypeBuilder<Recepcion> builder)
    {
        builder.ToTable("recepcion");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Combustible).IsRequired();
        builder.Property(r => r.Objetos).HasColumnType("text");
        builder.Property(r => r.Observaciones).HasColumnType("text");
        builder.Property(r => r.FirmaUrl).HasColumnType("text");

        builder.HasOne<OrdenTrabajo>().WithOne()
            .HasForeignKey<Recepcion>(r => new { r.EmpresaId, r.OtId })
            .HasPrincipalKey<OrdenTrabajo>(o => new { o.EmpresaId, o.Id })
            .OnDelete(DeleteBehavior.Restrict);
    }
}
