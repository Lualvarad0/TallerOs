using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TallerOS.Domain.Plataforma;

namespace TallerOS.Infrastructure.Persistence.Configuraciones.Plataforma;

public class PlanConfiguration : IEntityTypeConfiguration<Plan>
{
    public void Configure(EntityTypeBuilder<Plan> builder)
    {
        builder.ToTable("plan");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Nombre).HasMaxLength(60).IsRequired();

        // Nombre de columna explícito: la convención snake_case parte "WhatsApp" en dos palabras.
        builder.Property(p => p.PermiteWhatsAppOficial).HasColumnName("permite_whatsapp_oficial");

        builder.HasIndex(p => p.Nombre).IsUnique();

        // Catálogo fijo: los 3 planes de la especificación. Id fijo (no Guid.CreateVersion7()) porque
        // HasData necesita un valor estable entre ejecuciones de `dotnet ef migrations add`.
        builder.HasData(
            new
            {
                Id = Guid.Parse("916d3c96-5570-4817-9dcb-b21060472d9b"),
                Nombre = "Básico",
                LimiteUsuarios = (int?)3,
                LimiteSucursales = (int?)1,
                PermiteWhatsAppOficial = false,
                Activo = true,
            },
            new
            {
                Id = Guid.Parse("265aad84-c2a0-4e88-b9a8-d069b5c54dc4"),
                Nombre = "Pro",
                LimiteUsuarios = (int?)15,
                LimiteSucursales = (int?)3,
                PermiteWhatsAppOficial = true,
                Activo = true,
            },
            new
            {
                Id = Guid.Parse("b7b0a891-60cc-45ca-9cd2-99a9739dd529"),
                Nombre = "Empresa",
                LimiteUsuarios = (int?)null,
                LimiteSucursales = (int?)null,
                PermiteWhatsAppOficial = true,
                Activo = true,
            });
    }
}
