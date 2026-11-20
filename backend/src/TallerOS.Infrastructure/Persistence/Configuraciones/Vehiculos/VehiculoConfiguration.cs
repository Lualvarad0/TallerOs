using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TallerOS.Domain.Catalogos;
using TallerOS.Domain.Vehiculos;

namespace TallerOS.Infrastructure.Persistence.Configuraciones.Vehiculos;

public class VehiculoConfiguration : IEntityTypeConfiguration<Vehiculo>
{
    public void Configure(EntityTypeBuilder<Vehiculo> builder)
    {
        builder.ToTable("vehiculo");
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Vin).HasMaxLength(17);
        builder.Property(v => v.Motor).HasMaxLength(60);

        builder.HasOne<Marca>().WithMany().HasForeignKey(v => v.MarcaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Modelo>().WithMany().HasForeignKey(v => v.ModeloId).OnDelete(DeleteBehavior.Restrict);

        // Único pero nullable: Postgres no choca dos vehículos con vin NULL (VIN ilegible), que es
        // justo lo que se necesita.
        builder.HasIndex(v => v.Vin).IsUnique();
    }
}
