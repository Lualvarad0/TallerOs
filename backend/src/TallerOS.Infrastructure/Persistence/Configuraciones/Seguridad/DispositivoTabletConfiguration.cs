using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TallerOS.Domain.Organizacion;
using TallerOS.Domain.Seguridad;

namespace TallerOS.Infrastructure.Persistence.Configuraciones.Seguridad;

public class DispositivoTabletConfiguration : IEntityTypeConfiguration<DispositivoTablet>
{
    public void Configure(EntityTypeBuilder<DispositivoTablet> builder)
    {
        builder.ToTable("dispositivo_tablet");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Nombre).HasMaxLength(80).IsRequired();
        builder.Property(d => d.TokenHash).HasMaxLength(255).IsRequired();
        builder.Property(d => d.CreadoEn).IsRequired();

        builder.HasOne<Sucursal>()
            .WithMany()
            .HasForeignKey(d => new { d.EmpresaId, d.SucursalId })
            .HasPrincipalKey(s => new { s.EmpresaId, s.Id })
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(d => new { d.EmpresaId, d.SucursalId });

        // El dispositivo se autentica con su token antes de que se sepa a qué empresa pertenece
        // (igual razón que usuario_id en UsuarioRol), por eso es único a nivel global.
        builder.HasIndex(d => d.TokenHash).IsUnique();

        // Igual que el password_hash y el pin_hash de la semilla: no es un token real, el paso 3
        // define cómo se emite y valida.
        builder.HasData(new
        {
            Id = Guid.Parse("2d394826-38a4-45ea-8e53-eb9c4c72239c"),
            EmpresaId = Guid.Parse("e598ae6b-3823-446a-b9b3-3229deb3d134"),
            SucursalId = Guid.Parse("c648727f-1153-4fd1-b06a-3c1d6c1040cc"),
            Nombre = "Tablet recepción",
            TokenHash = "seed$no-es-un-token-real$reemplazar-en-paso-3",
            Activo = true,
            CreadoEn = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
        });
    }
}
