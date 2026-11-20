using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TallerOS.Domain.Organizacion;

namespace TallerOS.Infrastructure.Persistence.Configuraciones.Organizacion;

public class SucursalConfiguration : IEntityTypeConfiguration<Sucursal>
{
    public void Configure(EntityTypeBuilder<Sucursal> builder)
    {
        builder.ToTable("sucursal");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Nombre).HasMaxLength(160).IsRequired();
        builder.Property(s => s.Direccion).HasMaxLength(240);

        builder.HasOne<Empresa>().WithMany().HasForeignKey(s => s.EmpresaId).OnDelete(DeleteBehavior.Restrict);

        // Llave alterna (empresa_id, id): lo que permite que Area, Personal, UsuarioRol y
        // DispositivoTablet referencien una sucursal con FK compuesta y así sea imposible, a nivel
        // de base de datos, enlazar una fila con el empresa_id de otra empresa.
        builder.HasAlternateKey(s => new { s.EmpresaId, s.Id });

        builder.HasIndex(s => s.EmpresaId);
        builder.HasIndex(s => new { s.EmpresaId, s.Nombre }).IsUnique().HasFilter("eliminado_en IS NULL");

        builder.HasData(new
        {
            Id = Guid.Parse("c648727f-1153-4fd1-b06a-3c1d6c1040cc"),
            EmpresaId = Guid.Parse("e598ae6b-3823-446a-b9b3-3229deb3d134"),
            Nombre = "Principal",
            Direccion = "Av. Siempre Viva 123, Quito",
        });
    }
}
