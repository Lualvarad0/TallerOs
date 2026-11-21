using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TallerOS.Domain.OrdenesTrabajo;

namespace TallerOS.Infrastructure.Persistence.Configuraciones.OrdenesTrabajo;

public class ContadorNumeroOtConfiguration : IEntityTypeConfiguration<ContadorNumeroOt>
{
    public void Configure(EntityTypeBuilder<ContadorNumeroOt> builder)
    {
        builder.ToTable("contador_numero_ot");
        builder.HasKey(c => c.EmpresaId);

        builder.HasData(new
        {
            EmpresaId = Guid.Parse("e598ae6b-3823-446a-b9b3-3229deb3d134"),
            UltimoNumero = 0,
        });
    }
}
