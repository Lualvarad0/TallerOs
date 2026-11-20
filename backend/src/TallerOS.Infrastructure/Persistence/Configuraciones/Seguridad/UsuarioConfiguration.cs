using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TallerOS.Domain.Seguridad;

namespace TallerOS.Infrastructure.Persistence.Configuraciones.Seguridad;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuario");
        builder.HasKey(u => u.Id);

        // citext (ver infra/postgres/init/01-extensions.sql): comparación de email insensible a mayúsculas.
        builder.Property(u => u.Email).HasColumnType("citext").HasMaxLength(255).IsRequired();
        builder.Property(u => u.PasswordHash).HasMaxLength(255).IsRequired();
        builder.Property(u => u.CreadoEn).IsRequired();

        builder.HasIndex(u => u.Email).IsUnique();

        // Placeholder, no un hash real: el paso 3 (autenticación) define el esquema de hashing y
        // debe reemplazar esta semilla. Sirve solo para tener un usuario con el que probar RLS/EF.
        builder.HasData(new
        {
            Id = Guid.Parse("ce894d4e-9e31-492c-8230-8dba05250a94"),
            Email = "dueno@tallerdemo.test",
            PasswordHash = "seed$no-es-un-hash-real$reemplazar-en-paso-3",
            Activo = true,
            EsSuperAdmin = false,
            CreadoEn = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
        });
    }
}
