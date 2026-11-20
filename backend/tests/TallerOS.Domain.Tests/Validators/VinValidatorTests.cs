using TallerOS.Domain.Validators;

namespace TallerOS.Domain.Tests.Validators;

public sealed class VinValidatorTests
{
    [Fact]
    public void Vin_valido_pasa()
    {
        Assert.True(VinValidator.EsValido("1HGCM82633A004352"));
    }

    [Fact]
    public void Digito_verificador_incorrecto_no_pasa()
    {
        Assert.False(VinValidator.EsValido("1HGCM82633A004353"));
    }

    [Fact]
    public void Caracter_no_permitido_no_pasa()
    {
        // La I nunca se usa en un VIN (se confunde con el 1).
        Assert.False(VinValidator.EsValido("1HGCM82633AI04352"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("1HGCM82633A00435")]
    [InlineData("1HGCM82633A0043522")]
    public void Longitud_invalida_no_pasa(string vin)
    {
        Assert.False(VinValidator.EsValido(vin));
    }
}
