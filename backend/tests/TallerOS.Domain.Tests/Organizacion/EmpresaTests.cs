using TallerOS.Domain.Organizacion;

namespace TallerOS.Domain.Tests.Organizacion;

public sealed class EmpresaTests
{
    [Fact]
    public void El_constructor_arranca_Activa_con_id_propio()
    {
        var empresa = new Empresa("Taller Pérez", "1790012345001");

        Assert.NotEqual(Guid.Empty, empresa.Id);
        Assert.Equal(EstadoEmpresa.Activa, empresa.Estado);
        Assert.Null(empresa.EliminadoEn);
    }

    [Fact]
    public void ActualizarDatos_reemplaza_nombre_y_ruc()
    {
        var empresa = new Empresa("Taller Pérez", "1790012345001");

        empresa.ActualizarDatos("Taller Pérez e Hijos", "1790012345099");

        Assert.Equal("Taller Pérez e Hijos", empresa.Nombre);
        Assert.Equal("1790012345099", empresa.Ruc);
    }

    [Fact]
    public void CambiarEstado_actualiza_el_estado()
    {
        var empresa = new Empresa("Taller Pérez", "1790012345001");

        empresa.CambiarEstado(EstadoEmpresa.Suspendida);

        Assert.Equal(EstadoEmpresa.Suspendida, empresa.Estado);
    }
}
