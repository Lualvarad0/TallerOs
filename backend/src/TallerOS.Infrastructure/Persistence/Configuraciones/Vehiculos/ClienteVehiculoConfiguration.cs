using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TallerOS.Domain.Clientes;
using TallerOS.Domain.Vehiculos;

namespace TallerOS.Infrastructure.Persistence.Configuraciones.Vehiculos;

public class ClienteVehiculoConfiguration : IEntityTypeConfiguration<ClienteVehiculo>
{
    public void Configure(EntityTypeBuilder<ClienteVehiculo> builder)
    {
        builder.ToTable("cliente_vehiculo");
        builder.HasKey(cv => cv.Id);

        builder.Property(cv => cv.Rol).HasConversion<string>().HasMaxLength(20).IsRequired();

        // FK compuesta contra (empresa_id, id) de cliente: a nivel de base de datos es imposible
        // vincular un vehículo al cliente de otra empresa, sin depender de que el código lo valide.
        builder.HasOne<Cliente>()
            .WithMany()
            .HasForeignKey(cv => new { cv.EmpresaId, cv.ClienteId })
            .HasPrincipalKey(c => new { c.EmpresaId, c.Id })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Vehiculo>().WithMany().HasForeignKey(cv => cv.VehiculoId).OnDelete(DeleteBehavior.Restrict);

        // Un cliente no se vincula dos veces al mismo vehículo (el rol ya distingue el papel que juega).
        builder.HasIndex(cv => new { cv.EmpresaId, cv.ClienteId, cv.VehiculoId }).IsUnique();

        // Esta es la consulta que hacen las políticas RLS de vehiculo y vehiculo_placa (¿esta
        // empresa tiene un vínculo con este vehículo?); vehiculo_id primero porque se filtra por un
        // vehículo concreto, no por "todos los de mi empresa".
        builder.HasIndex(cv => new { cv.VehiculoId, cv.EmpresaId }).HasDatabaseName("ix_cliente_vehiculo_vehiculo_id_empresa_id");
    }
}
