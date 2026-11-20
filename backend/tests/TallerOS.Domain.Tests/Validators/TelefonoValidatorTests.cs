using TallerOS.Domain.Validators;

namespace TallerOS.Domain.Tests.Validators;

public sealed class TelefonoValidatorTests
{
    [Theory]
    [InlineData("0991234567")] // celular, formato local
    [InlineData("+593991234567")] // celular, E.164
    [InlineData("022345678")] // convencional Quito, formato local
    [InlineData("+59322345678")] // convencional Quito, E.164
    public void Numero_valido_pasa(string telefono)
    {
        Assert.True(TelefonoValidator.EsValido(telefono));
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("099123456")] // celular, un dígito corto
    [InlineData("+15551234567")] // número de otro país
    public void Numero_invalido_no_pasa(string telefono)
    {
        Assert.False(TelefonoValidator.EsValido(telefono));
    }
}
