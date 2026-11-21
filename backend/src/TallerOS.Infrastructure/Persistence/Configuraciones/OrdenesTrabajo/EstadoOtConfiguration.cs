using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TallerOS.Domain.OrdenesTrabajo;

namespace TallerOS.Infrastructure.Persistence.Configuraciones.OrdenesTrabajo;

public class EstadoOtConfiguration : IEntityTypeConfiguration<EstadoOt>
{
    private static readonly Guid EmpresaDemoId = Guid.Parse("e598ae6b-3823-446a-b9b3-3229deb3d134");

    public void Configure(EntityTypeBuilder<EstadoOt> builder)
    {
        builder.ToTable("estado_ot");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Nombre).HasMaxLength(40).IsRequired();
        builder.Property(e => e.ColorToken).HasMaxLength(20).IsRequired();

        builder.HasIndex(e => new { e.EmpresaId, e.Orden }).IsUnique();

        // Los 8 estados estándar del flujo (ver docs/especificacion.md y docs/diseno/README.md §
        // Mapeo de estados de OT → color), sembrados solo para la empresa demo. Para empresas
        // nuevas los siembra CrearEmpresaCommandHandler (paso de Backend), no esta migración.
        builder.HasData(
            new { Id = Guid.Parse("00000004-aaaa-4000-8000-000000000001"), EmpresaId = EmpresaDemoId, Nombre = "Recibido", Orden = (short)1, EsFinal = false, ColorToken = "sf3" },
            new { Id = Guid.Parse("00000004-aaaa-4000-8000-000000000002"), EmpresaId = EmpresaDemoId, Nombre = "Diagnóstico", Orden = (short)2, EsFinal = false, ColorToken = "blue" },
            new { Id = Guid.Parse("00000004-aaaa-4000-8000-000000000003"), EmpresaId = EmpresaDemoId, Nombre = "Esperando aprobación del cliente", Orden = (short)3, EsFinal = false, ColorToken = "vio" },
            new { Id = Guid.Parse("00000004-aaaa-4000-8000-000000000004"), EmpresaId = EmpresaDemoId, Nombre = "Esperando repuestos", Orden = (short)4, EsFinal = false, ColorToken = "vio" },
            new { Id = Guid.Parse("00000004-aaaa-4000-8000-000000000005"), EmpresaId = EmpresaDemoId, Nombre = "En reparación", Orden = (short)5, EsFinal = false, ColorToken = "blue" },
            new { Id = Guid.Parse("00000004-aaaa-4000-8000-000000000006"), EmpresaId = EmpresaDemoId, Nombre = "Control de calidad", Orden = (short)6, EsFinal = false, ColorToken = "blue" },
            new { Id = Guid.Parse("00000004-aaaa-4000-8000-000000000007"), EmpresaId = EmpresaDemoId, Nombre = "Listo para entregar", Orden = (short)7, EsFinal = false, ColorToken = "grn" },
            new { Id = Guid.Parse("00000004-aaaa-4000-8000-000000000008"), EmpresaId = EmpresaDemoId, Nombre = "Entregado", Orden = (short)8, EsFinal = true, ColorToken = "grn" }
        );
    }
}
