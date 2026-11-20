using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TallerOS.Domain.Clientes;
using TallerOS.Domain.Vehiculos;

namespace TallerOS.Infrastructure.Persistence.Configuraciones.Vehiculos;

public class VehiculoPropietarioConfiguration : IEntityTypeConfiguration<VehiculoPropietario>
{
    public void Configure(EntityTypeBuilder<VehiculoPropietario> builder)
    {
        builder.ToTable("vehiculo_propietario");
        builder.HasKey(vp => vp.Id);

        builder.HasOne<Vehiculo>().WithMany().HasForeignKey(vp => vp.VehiculoId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<CuentaCliente>().WithMany().HasForeignKey(vp => vp.CuentaClienteId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(vp => vp.VehiculoId);
    }
}
