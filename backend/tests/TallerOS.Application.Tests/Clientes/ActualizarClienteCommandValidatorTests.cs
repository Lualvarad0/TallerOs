using TallerOS.Application.Clientes.Commands.ActualizarCliente;
using TallerOS.Domain.Clientes;

namespace TallerOS.Application.Tests.Clientes;

public sealed class ActualizarClienteCommandValidatorTests
{
    private readonly ActualizarClienteCommandValidator _validator = new();

    private static ActualizarClienteCommand Comando(
        string nombre = "Cliente de prueba", string? telefono = "0991234567", string? email = "cliente@correo.com") =>
        new(Guid.NewGuid(), nombre, telefono, email, CanalPreferido.WhatsApp);

    [Fact]
    public void Comando_valido_pasa()
    {
        var resultado = _validator.Validate(Comando());

        Assert.True(resultado.IsValid);
    }

    [Fact]
    public void Sin_telefono_ni_email_pasa_igual()
    {
        var resultado = _validator.Validate(Comando(telefono: null, email: null));

        Assert.True(resultado.IsValid);
    }

    [Fact]
    public void Nombre_vacio_no_pasa()
    {
        var resultado = _validator.Validate(Comando(nombre: ""));

        Assert.False(resultado.IsValid);
    }

    [Fact]
    public void Telefono_invalido_no_pasa()
    {
        var resultado = _validator.Validate(Comando(telefono: "123"));

        Assert.False(resultado.IsValid);
    }

    [Fact]
    public void Email_invalido_no_pasa()
    {
        var resultado = _validator.Validate(Comando(email: "no-es-un-correo"));

        Assert.False(resultado.IsValid);
    }

    [Fact]
    public void ClienteId_vacio_no_pasa()
    {
        var resultado = _validator.Validate(Comando() with { ClienteId = Guid.Empty });

        Assert.False(resultado.IsValid);
    }
}
