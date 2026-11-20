using TallerOS.Application.Clientes.Commands.CrearCliente;
using TallerOS.Domain.Clientes;

namespace TallerOS.Application.Tests.Clientes;

public sealed class CrearClienteCommandValidatorTests
{
    private readonly CrearClienteCommandValidator _validator = new();

    private static CrearClienteCommand Comando(
        TipoCliente tipo = TipoCliente.PersonaNatural,
        string identificacion = "1710034065",
        string? telefono = "0991234567",
        string? email = "cliente@correo.com") =>
        new("Cliente de prueba", tipo, identificacion, telefono, email, CanalPreferido.WhatsApp);

    [Fact]
    public void Persona_natural_con_cedula_valida_pasa()
    {
        var resultado = _validator.Validate(Comando());

        Assert.True(resultado.IsValid);
    }

    [Fact]
    public void Persona_natural_con_cedula_invalida_no_pasa()
    {
        var resultado = _validator.Validate(Comando(identificacion: "1710034060"));

        Assert.False(resultado.IsValid);
    }

    [Fact]
    public void Persona_natural_con_ruc_de_persona_natural_pasa()
    {
        // Profesional independiente que factura con su propio RUC (tercer dígito 0-5: cédula +
        // establecimiento, ver RucValidatorTests) sin tener que registrarse como Tipo.Empresa.
        var resultado = _validator.Validate(Comando(identificacion: "1710034065001"));

        Assert.True(resultado.IsValid);
    }

    [Fact]
    public void Persona_natural_con_ruc_de_sociedad_no_pasa()
    {
        // Tercer dígito 9 (sociedad privada, ver RucValidatorTests): ese RUC identifica una empresa,
        // no a la persona natural dueña del cliente.
        var resultado = _validator.Validate(Comando(identificacion: "1791234561001"));

        Assert.False(resultado.IsValid);
    }

    [Fact]
    public void Empresa_con_ruc_de_sociedad_valido_pasa()
    {
        var resultado = _validator.Validate(Comando(tipo: TipoCliente.Empresa, identificacion: "1791234561001"));

        Assert.True(resultado.IsValid);
    }

    [Fact]
    public void Empresa_con_ruc_invalido_no_pasa()
    {
        var resultado = _validator.Validate(Comando(tipo: TipoCliente.Empresa, identificacion: "1791234562001"));

        Assert.False(resultado.IsValid);
    }

    [Fact]
    public void Empresa_con_ruc_de_persona_natural_no_pasa()
    {
        // Checksum válido (ver RucValidatorTests), pero tercer dígito 0-5 es de persona natural, no
        // de una sociedad: no identifica a una Empresa aunque el RUC en sí sea válido.
        var resultado = _validator.Validate(Comando(tipo: TipoCliente.Empresa, identificacion: "1710034065001"));

        Assert.False(resultado.IsValid);
    }

    [Fact]
    public void Sin_telefono_ni_email_pasa_igual()
    {
        // Ambos opcionales (ver Cliente): el canal preferido puede ser WhatsApp sin que el teléfono
        // esté registrado todavía, por ejemplo.
        var resultado = _validator.Validate(Comando(telefono: null, email: null));

        Assert.True(resultado.IsValid);
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
}
