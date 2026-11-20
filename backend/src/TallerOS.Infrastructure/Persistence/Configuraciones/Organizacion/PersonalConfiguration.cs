using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TallerOS.Domain.Organizacion;
using TallerOS.Domain.Seguridad;

namespace TallerOS.Infrastructure.Persistence.Configuraciones.Organizacion;

public class PersonalConfiguration : IEntityTypeConfiguration<Personal>
{
    public void Configure(EntityTypeBuilder<Personal> builder)
    {
        builder.ToTable("personal");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Nombre).HasMaxLength(160).IsRequired();
        builder.Property(p => p.TarifaHora).HasColumnType("numeric(10,2)");
        builder.Property(p => p.PinHash).HasMaxLength(255);

        builder.HasOne<Area>()
            .WithMany()
            .HasForeignKey(p => new { p.EmpresaId, p.AreaId })
            .HasPrincipalKey(a => new { a.EmpresaId, a.Id })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Usuario>().WithMany().HasForeignKey(p => p.UsuarioId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => new { p.EmpresaId, p.AreaId });

        builder.HasData(
            new
            {
                Id = Guid.Parse("d0ad8cc9-30b6-4ddf-a583-5ff660b91ce7"),
                EmpresaId = Guid.Parse("e598ae6b-3823-446a-b9b3-3229deb3d134"),
                AreaId = Guid.Parse("b7ae6d86-0fd3-40d9-bc12-c31f6a37011f"), // Administración
                Nombre = "Ana Dueña",
                TarifaHora = (decimal?)null,
                TieneAcceso = true,
                UsuarioId = (Guid?)Guid.Parse("ce894d4e-9e31-492c-8230-8dba05250a94"),
                PinHash = (string?)null,
            },
            new
            {
                // Mecánico sin cuenta propia: ficha con PIN en la tablet del taller
                // (ver DispositivoTablet). Mismo comentario que en UsuarioConfiguration: el hash
                // real de PIN lo define el paso 3, esto es solo un valor de prueba.
                Id = Guid.Parse("b040b030-bef2-4bf6-8a40-a5fce884f306"),
                EmpresaId = Guid.Parse("e598ae6b-3823-446a-b9b3-3229deb3d134"),
                AreaId = Guid.Parse("12ce199f-0fb5-4b45-a0ec-6f6ad26c2a92"), // Mecánica
                Nombre = "Pedro Mecánico",
                TarifaHora = (decimal?)4.50m,
                TieneAcceso = false,
                UsuarioId = (Guid?)null,
                PinHash = (string?)"seed$no-es-un-hash-real$reemplazar-en-paso-3",
            });
    }
}
