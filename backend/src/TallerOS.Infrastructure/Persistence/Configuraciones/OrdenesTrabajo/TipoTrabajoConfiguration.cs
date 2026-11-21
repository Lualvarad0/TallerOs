using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TallerOS.Domain.OrdenesTrabajo;

namespace TallerOS.Infrastructure.Persistence.Configuraciones.OrdenesTrabajo;

public class TipoTrabajoConfiguration : IEntityTypeConfiguration<TipoTrabajo>
{
    public void Configure(EntityTypeBuilder<TipoTrabajo> builder)
    {
        builder.ToTable("tipo_trabajo");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Nombre).HasMaxLength(80).IsRequired();

        builder.HasIndex(t => new { t.EmpresaId, t.Nombre }).IsUnique().HasFilter("eliminado_en IS NULL");
    }
}
