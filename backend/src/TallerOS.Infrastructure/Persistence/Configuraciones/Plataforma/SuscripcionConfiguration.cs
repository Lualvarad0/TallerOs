using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TallerOS.Domain.Organizacion;
using TallerOS.Domain.Plataforma;

namespace TallerOS.Infrastructure.Persistence.Configuraciones.Plataforma;

public class SuscripcionConfiguration : IEntityTypeConfiguration<Suscripcion>
{
    public void Configure(EntityTypeBuilder<Suscripcion> builder)
    {
        builder.ToTable("suscripcion");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Estado).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(s => s.VigenteDesde).IsRequired();

        builder.HasOne<Empresa>().WithMany().HasForeignKey(s => s.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Plan>().WithMany().HasForeignKey(s => s.PlanId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => s.EmpresaId).HasDatabaseName("ix_suscripcion_empresa_id");

        // Una sola suscripción activa a la vez por empresa. Nombre propio para que EF no la funda
        // con el índice de arriba: mismas columnas, pero son dos índices distintos.
        builder.HasIndex(s => s.EmpresaId)
            .IsUnique()
            .HasFilter("estado = 'Activa'")
            .HasDatabaseName("ux_suscripcion_empresa_id_activa");

        builder.HasData(new
        {
            Id = Guid.Parse("a5e5f8f6-a35b-4ffc-ad0d-0fddc519bcf2"),
            EmpresaId = Guid.Parse("e598ae6b-3823-446a-b9b3-3229deb3d134"),
            PlanId = Guid.Parse("265aad84-c2a0-4e88-b9a8-d069b5c54dc4"), // Pro
            Estado = EstadoSuscripcion.Activa,
            VigenteDesde = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
            VigenteHasta = (DateTimeOffset?)null,
        });
    }
}
