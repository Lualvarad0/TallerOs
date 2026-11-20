using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TallerOS.Domain.Catalogos;

namespace TallerOS.Infrastructure.Persistence.Configuraciones.Catalogos;

public class MarcaConfiguration : IEntityTypeConfiguration<Marca>
{
    public void Configure(EntityTypeBuilder<Marca> builder)
    {
        builder.ToTable("marca");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Nombre).HasMaxLength(80).IsRequired();

        builder.HasOne<TipoVehiculo>().WithMany().HasForeignKey(m => m.TipoVehiculoId).OnDelete(DeleteBehavior.Restrict);

        // Bloquea el duplicado exacto ("Toyota" dos veces para Auto); el parecido no exacto
        // ("Toyotaa") lo revisa la app con el índice de trigramas de abajo antes de insertar.
        builder.HasIndex(m => new { m.TipoVehiculoId, m.Nombre }).IsUnique();

        builder.HasIndex(m => m.Nombre)
            .HasDatabaseName("ix_marca_nombre_trgm")
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");
    }
}
