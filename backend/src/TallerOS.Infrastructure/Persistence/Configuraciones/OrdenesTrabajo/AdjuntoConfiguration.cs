using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TallerOS.Domain.OrdenesTrabajo;
using TallerOS.Domain.Seguridad;

namespace TallerOS.Infrastructure.Persistence.Configuraciones.OrdenesTrabajo;

public class AdjuntoConfiguration : IEntityTypeConfiguration<Adjunto>
{
    public void Configure(EntityTypeBuilder<Adjunto> builder)
    {
        builder.ToTable("adjunto");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Entidad).HasMaxLength(30).IsRequired();
        builder.Property(a => a.Tipo).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(a => a.Url).HasColumnType("text").IsRequired();
        builder.Property(a => a.MiniaturaUrl).HasColumnType("text");
        builder.Property(a => a.CreadoEn).IsRequired();

        // Sin llave foránea hacia la entidad referenciada: ver el comentario de Adjunto en Domain.
        builder.HasOne<Usuario>().WithMany().HasForeignKey(a => a.AutorUsuarioId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => new { a.EmpresaId, a.Entidad, a.EntidadId });
    }
}
