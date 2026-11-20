using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TallerOS.Domain.Catalogos;

namespace TallerOS.Infrastructure.Persistence.Configuraciones.Catalogos;

public class ModeloConfiguration : IEntityTypeConfiguration<Modelo>
{
    public void Configure(EntityTypeBuilder<Modelo> builder)
    {
        builder.ToTable("modelo");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Nombre).HasMaxLength(100).IsRequired();

        builder.HasOne<Marca>().WithMany().HasForeignKey(m => m.MarcaId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(m => new { m.MarcaId, m.Nombre }).IsUnique();

        builder.HasIndex(m => m.Nombre)
            .HasDatabaseName("ix_modelo_nombre_trgm")
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");
    }
}
