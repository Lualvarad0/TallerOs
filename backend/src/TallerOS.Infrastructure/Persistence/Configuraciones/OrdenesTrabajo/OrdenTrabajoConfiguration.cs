using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TallerOS.Domain.Clientes;
using TallerOS.Domain.Organizacion;
using TallerOS.Domain.OrdenesTrabajo;
using TallerOS.Domain.Vehiculos;

namespace TallerOS.Infrastructure.Persistence.Configuraciones.OrdenesTrabajo;

public class OrdenTrabajoConfiguration : IEntityTypeConfiguration<OrdenTrabajo>
{
    public void Configure(EntityTypeBuilder<OrdenTrabajo> builder)
    {
        builder.ToTable("orden_trabajo");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Prioridad).HasConversion<string>().HasMaxLength(10).IsRequired();
        builder.Property(o => o.IngresoEn).IsRequired();

        // Mismo motivo que ClienteVehiculoConfiguration: FK compuesta contra (empresa_id, id) para
        // que la base impida, no solo el código, apuntar a la sucursal/cliente/estado de otra empresa.
        builder.HasOne<Sucursal>().WithMany()
            .HasForeignKey(o => new { o.EmpresaId, o.SucursalId })
            .HasPrincipalKey(s => new { s.EmpresaId, s.Id })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Cliente>().WithMany()
            .HasForeignKey(o => new { o.EmpresaId, o.ClienteId })
            .HasPrincipalKey(c => new { c.EmpresaId, c.Id })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<EstadoOt>().WithMany()
            .HasForeignKey(o => new { o.EmpresaId, o.EstadoId })
            .HasPrincipalKey(e => new { e.EmpresaId, e.Id })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Personal>().WithMany()
            .HasForeignKey(o => new { o.EmpresaId, o.AsesorPersonalId })
            .HasPrincipalKey(p => new { p.EmpresaId, p.Id })
            .OnDelete(DeleteBehavior.Restrict);

        // Vehiculo es catálogo global (sin empresa_id, ver docs/convenciones.md § Seguridad): FK simple.
        builder.HasOne<Vehiculo>().WithMany().HasForeignKey(o => o.VehiculoId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(o => new { o.EmpresaId, o.Numero }).IsUnique();

        // La consulta del tablero Kanban: todas las OT de la empresa agrupadas por estado, con lo
        // que pinta la tarjeta sin volver a golpear la tabla.
        builder.HasIndex(o => new { o.EmpresaId, o.EstadoId })
            .IncludeProperties(o => new { o.VehiculoId, o.ClienteId, o.Prioridad, o.IngresoEn });

        // Historial de OT de un vehículo (ficha de vehículo) y de un cliente.
        builder.HasIndex(o => new { o.EmpresaId, o.VehiculoId });
        builder.HasIndex(o => new { o.EmpresaId, o.ClienteId });
    }
}
