using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TallerOS.Domain.Clientes;
using TallerOS.Domain.Seguridad;

namespace TallerOS.Infrastructure.Persistence.Configuraciones.Clientes;

public class CuentaClienteConfiguration : IEntityTypeConfiguration<CuentaCliente>
{
    public void Configure(EntityTypeBuilder<CuentaCliente> builder)
    {
        builder.ToTable("cuenta_cliente");
        builder.HasKey(cc => cc.Id);

        builder.Property(cc => cc.Telefono).HasMaxLength(16).IsRequired();

        builder.HasOne<Usuario>().WithMany().HasForeignKey(cc => cc.UsuarioId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(cc => cc.UsuarioId).IsUnique();
    }
}
