using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TallerOS.Domain.Vehiculos;

namespace TallerOS.Infrastructure.Persistence.Configuraciones.Vehiculos;

public class VehiculoPlacaConfiguration : IEntityTypeConfiguration<VehiculoPlaca>
{
    public void Configure(EntityTypeBuilder<VehiculoPlaca> builder)
    {
        builder.ToTable("vehiculo_placa");
        builder.HasKey(vp => vp.Id);

        builder.Property(vp => vp.Placa).HasMaxLength(10).IsRequired();

        builder.HasOne<Vehiculo>().WithMany().HasForeignKey(vp => vp.VehiculoId).OnDelete(DeleteBehavior.Restrict);

        // Una sola placa activa a la vez en todo el sistema (hasta nulo = activa); evita que dos
        // vehículos distintos queden con la misma placa vigente. El nombre en HasIndex (no solo en
        // HasDatabaseName) es necesario: dos llamadas sobre la misma columna sin nombre propio las
        // trata EF Core como el mismo índice y la segunda pisa a la primera.
        builder.HasIndex(vp => vp.Placa, "ix_vehiculo_placa_placa_activa")
            .HasDatabaseName("ix_vehiculo_placa_placa_activa")
            .IsUnique()
            .HasFilter("hasta IS NULL");

        // Búsqueda parcial por placa en recepción (ver especificación, Clientes con varios vehículos).
        builder.HasIndex(vp => vp.Placa, "ix_vehiculo_placa_placa_trgm")
            .HasDatabaseName("ix_vehiculo_placa_placa_trgm")
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");
    }
}
