using TallerOS.Domain.Validators;

namespace TallerOS.Domain.Tests.Validators;

public sealed class PlacaValidatorTests
{
    [Theory]
    [InlineData("ABC-1234")] // auto, 3 letras
    [InlineData("AB-1234")] // moto, 2 letras
    [InlineData("abc-1234")] // minúsculas, igual válido
    public void Formato_reconocido_pasa(string placa)
    {
        Assert.True(PlacaValidator.TieneFormatoReconocido(placa));
    }

    [Theory]
    [InlineData("")]
    [InlineData("ABC1234")] // sin guion
    [InlineData("ABCD-1234")] // 4 letras
    [InlineData("AB-12")] // muy corta
    public void Formato_no_reconocido_no_pasa(string placa)
    {
        Assert.False(PlacaValidator.TieneFormatoReconocido(placa));
    }
}
