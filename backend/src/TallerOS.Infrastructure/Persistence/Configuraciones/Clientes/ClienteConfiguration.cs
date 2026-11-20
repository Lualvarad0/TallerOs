using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TallerOS.Domain.Clientes;
using TallerOS.Domain.Organizacion;

namespace TallerOS.Infrastructure.Persistence.Configuraciones.Clientes;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("cliente");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Nombre).HasMaxLength(160).IsRequired();
        builder.Property(c => c.Tipo).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(c => c.Identificacion).HasMaxLength(13).IsRequired();
        builder.Property(c => c.Telefono).HasMaxLength(16);
        builder.Property(c => c.Email).HasMaxLength(160);
        builder.Property(c => c.CanalPreferido).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.HasOne<Empresa>().WithMany().HasForeignKey(c => c.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<CuentaCliente>().WithMany().HasForeignKey(c => c.CuentaClienteId).OnDelete(DeleteBehavior.Restrict);

        // Llave alterna (empresa_id, id): lo que permite que ClienteVehiculo referencie un cliente
        // con FK compuesta, igual que Sucursal/Area. Así es imposible, a nivel de base de datos,
        // vincular un vehículo al cliente de otra empresa aunque el código de la Api tenga un bug.
        builder.HasAlternateKey(c => new { c.EmpresaId, c.Id });

        builder.HasIndex(c => c.EmpresaId);
        builder.HasIndex(c => new { c.EmpresaId, c.Identificacion }).IsUnique().HasFilter("eliminado_en IS NULL");

        // Búsqueda parcial en recepción: "por placa, cédula o nombre" (ver especificación, Clientes
        // con varios vehículos).
        builder.HasIndex(c => c.Nombre).HasDatabaseName("ix_cliente_nombre_trgm").HasMethod("gin").HasOperators("gin_trgm_ops");
        builder.HasIndex(c => c.Identificacion).HasDatabaseName("ix_cliente_identificacion_trgm").HasMethod("gin").HasOperators("gin_trgm_ops");
    }
}
