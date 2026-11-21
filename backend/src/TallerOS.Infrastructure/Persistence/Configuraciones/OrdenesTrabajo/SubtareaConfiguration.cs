using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TallerOS.Domain.Organizacion;
using TallerOS.Domain.OrdenesTrabajo;

namespace TallerOS.Infrastructure.Persistence.Configuraciones.OrdenesTrabajo;

public class SubtareaConfiguration : IEntityTypeConfiguration<Subtarea>
{
    public void Configure(EntityTypeBuilder<Subtarea> builder)
    {
        builder.ToTable("subtarea");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Estado).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.HasOne<OrdenTrabajo>().WithMany()
            .HasForeignKey(s => new { s.EmpresaId, s.OtId })
            .HasPrincipalKey(o => new { o.EmpresaId, o.Id })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<TipoTrabajo>().WithMany()
            .HasForeignKey(s => new { s.EmpresaId, s.TipoTrabajoId })
            .HasPrincipalKey(t => new { t.EmpresaId, t.Id })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Personal>().WithMany()
            .HasForeignKey(s => new { s.EmpresaId, s.ResponsablePersonalId })
            .HasPrincipalKey(p => new { p.EmpresaId, p.Id })
            .OnDelete(DeleteBehavior.Restrict);

        // Listar las subtareas de una OT en el orden en que se muestran en el detalle.
        builder.HasIndex(s => new { s.EmpresaId, s.OtId, s.Orden });
    }
}
