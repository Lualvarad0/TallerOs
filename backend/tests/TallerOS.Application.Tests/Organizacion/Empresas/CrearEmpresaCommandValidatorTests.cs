using TallerOS.Application.Organizacion.Empresas.Commands.CrearEmpresa;

namespace TallerOS.Application.Tests.Organizacion.Empresas;

public sealed class CrearEmpresaCommandValidatorTests
{
    private readonly CrearEmpresaCommandValidator _validator = new();

    private static CrearEmpresaCommand Comando(string ruc) =>
        new("Taller Pérez", ruc, Guid.NewGuid(), "dueño@taller.com", "clave-segura-123");

    [Fact]
    public void Ruc_de_13_digitos_pasa()
    {
        var resultado = _validator.Validate(Comando("1790012345001"));

        Assert.True(resultado.IsValid);
    }

    [Theory]
    [InlineData("179001234500")] // 12 dígitos
    [InlineData("17900123450011")] // 14 dígitos
    [InlineData("179001234500A")] // no es solo numérico
    public void Ruc_que_no_tiene_exactamente_13_digitos_no_pasa(string ruc)
    {
        // Solo valida formato (13 dígitos); el dígito verificador real es del Módulo 2.
        var resultado = _validator.Validate(Comando(ruc));

        Assert.False(resultado.IsValid);
    }
}
