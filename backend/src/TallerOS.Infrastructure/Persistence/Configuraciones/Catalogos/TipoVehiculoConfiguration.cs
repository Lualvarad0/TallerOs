using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TallerOS.Domain.Catalogos;

namespace TallerOS.Infrastructure.Persistence.Configuraciones.Catalogos;

public class TipoVehiculoConfiguration : IEntityTypeConfiguration<TipoVehiculo>
{
    public void Configure(EntityTypeBuilder<TipoVehiculo> builder)
    {
        builder.ToTable("tipo_vehiculo");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id).HasColumnType("smallint").ValueGeneratedNever();
        builder.Property(t => t.Nombre).HasMaxLength(30).IsRequired();

        builder.HasIndex(t => t.Nombre).IsUnique();

        // Los 5 tipos de la especificación (docs/especificacion.md § Motos). Cada empresa elige
        // cuáles atiende (configuración de Módulo 1), pero el catálogo en sí es el mismo para todas.
        builder.HasData(
            new TipoVehiculo(1, "Auto"),
            new TipoVehiculo(2, "Camioneta/SUV"),
            new TipoVehiculo(3, "Moto"),
            new TipoVehiculo(4, "Camión"),
            new TipoVehiculo(5, "Otro"));
    }
}
