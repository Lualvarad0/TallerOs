using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TallerOS.Domain.OrdenesTrabajo;
using TallerOS.Domain.Seguridad;

namespace TallerOS.Infrastructure.Persistence.Configuraciones.OrdenesTrabajo;

public class HistorialEstadoOtConfiguration : IEntityTypeConfiguration<HistorialEstadoOt>
{
    public void Configure(EntityTypeBuilder<HistorialEstadoOt> builder)
    {
        builder.ToTable("historial_estado_ot");
        builder.HasKey(h => h.Id);

        builder.Property(h => h.Fecha).IsRequired();

        builder.HasOne<OrdenTrabajo>().WithMany()
            .HasForeignKey(h => new { h.EmpresaId, h.OtId })
            .HasPrincipalKey(o => new { o.EmpresaId, o.Id })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<EstadoOt>().WithMany()
            .HasForeignKey(h => new { h.EmpresaId, h.DeEstadoId })
            .HasPrincipalKey(e => new { e.EmpresaId, e.Id })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<EstadoOt>().WithMany()
            .HasForeignKey(h => new { h.EmpresaId, h.AEstadoId })
            .HasPrincipalKey(e => new { e.EmpresaId, e.Id })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Usuario>().WithMany().HasForeignKey(h => h.UsuarioId).OnDelete(DeleteBehavior.Restrict);

        // Ver el historial de una OT ordenado por fecha, de más reciente a más antigua.
        builder.HasIndex(h => new { h.EmpresaId, h.OtId, h.Fecha });
    }
}
