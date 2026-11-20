using TallerOS.Application.Seguridad.Autenticacion.Commands.IniciarSesion;

namespace TallerOS.Application.Tests.Seguridad.Autenticacion;

public sealed class IniciarSesionCommandValidatorTests
{
    private readonly IniciarSesionCommandValidator _validator = new();

    [Fact]
    public void Email_y_password_validos_pasan()
    {
        var resultado = _validator.Validate(new IniciarSesionCommand("dueño@taller.com", "clave-segura"));

        Assert.True(resultado.IsValid);
    }

    [Theory]
    [InlineData("", "clave-segura")]
    [InlineData("no-es-un-email", "clave-segura")]
    [InlineData("dueño@taller.com", "")]
    public void Email_invalido_o_password_vacio_no_pasan(string email, string password)
    {
        var resultado = _validator.Validate(new IniciarSesionCommand(email, password));

        Assert.False(resultado.IsValid);
    }
}
