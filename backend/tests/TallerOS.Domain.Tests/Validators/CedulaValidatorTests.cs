using TallerOS.Domain.Validators;

namespace TallerOS.Domain.Tests.Validators;

public sealed class CedulaValidatorTests
{
    [Fact]
    public void Cedula_valida_pasa()
    {
        Assert.True(CedulaValidator.EsValida("1710034065"));
    }

    [Fact]
    public void Digito_verificador_incorrecto_no_pasa()
    {
        Assert.False(CedulaValidator.EsValida("1710034060"));
    }

    [Fact]
    public void Provincia_fuera_de_rango_no_pasa()
    {
        Assert.False(CedulaValidator.EsValida("9910034068"));
    }

    [Fact]
    public void Tercer_digito_de_RUC_no_de_cedula_no_pasa()
    {
        // Checksum correcto (verificador 4), pero el tercer dígito (6) es de entidad pública, no
        // de persona natural.
        Assert.False(CedulaValidator.EsValida("1760034064"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("12345")]
    [InlineData("17100340A5")]
    public void Formato_invalido_no_pasa(string cedula)
    {
        Assert.False(CedulaValidator.EsValida(cedula));
    }
}
