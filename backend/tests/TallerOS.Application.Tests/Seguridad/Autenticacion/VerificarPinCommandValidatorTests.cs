using TallerOS.Application.Seguridad.Autenticacion.Commands.VerificarPin;

namespace TallerOS.Application.Tests.Seguridad.Autenticacion;

public sealed class VerificarPinCommandValidatorTests
{
    private readonly VerificarPinCommandValidator _validator = new();

    [Fact]
    public void Token_y_pin_no_vacios_pasan()
    {
        var resultado = _validator.Validate(new VerificarPinCommand("token-de-prueba", "1234"));

        Assert.True(resultado.IsValid);
    }

    [Theory]
    [InlineData("", "1234")]
    [InlineData("token-de-prueba", "")]
    public void Token_o_pin_vacio_no_pasan(string token, string pin)
    {
        var resultado = _validator.Validate(new VerificarPinCommand(token, pin));

        Assert.False(resultado.IsValid);
    }
}
