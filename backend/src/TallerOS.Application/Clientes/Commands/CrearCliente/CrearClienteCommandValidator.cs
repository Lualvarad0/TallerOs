using FluentValidation;
using TallerOS.Domain.Clientes;
using TallerOS.Domain.Validators;

namespace TallerOS.Application.Clientes.Commands.CrearCliente;

public sealed class CrearClienteCommandValidator : AbstractValidator<CrearClienteCommand>
{
    public CrearClienteCommandValidator()
    {
        RuleFor(c => c.Nombre).NotEmpty().MaximumLength(160);
        RuleFor(c => c.Tipo).IsInEnum();

        // Persona natural se identifica con cédula (10 dígitos) O, si se le factura con RUC (un
        // profesional independiente, por ejemplo), con el RUC de persona natural (13 dígitos, tercer
        // dígito 0-5: cédula + establecimiento, ver RucValidator) — sin obligarla a registrarse como
        // Tipo.Empresa, que es para sociedades de verdad. Empresa exige el RUC de una sociedad
        // (tercer dígito 6 entidad pública o 9 privada): un RUC de persona natural no la identifica
        // como empresa, aunque el checksum sea válido.
        RuleFor(c => c.Identificacion).NotEmpty()
            .Must(id => CedulaValidator.EsValida(id) || (EsRucDePersonaNatural(id) && RucValidator.EsValido(id)))
            .WithMessage("La identificación debe ser una cédula válida o, si se factura con RUC, un RUC de persona natural válido.")
            .When(c => c.Tipo == TipoCliente.PersonaNatural);
        RuleFor(c => c.Identificacion).NotEmpty()
            .Must(id => EsRucDeSociedad(id) && RucValidator.EsValido(id))
            .WithMessage("El RUC no es válido.")
            .When(c => c.Tipo == TipoCliente.Empresa);

        RuleFor(c => c.Telefono).Must(TelefonoValidator.EsValido)
            .WithMessage("El teléfono no es válido.")
            .When(c => !string.IsNullOrWhiteSpace(c.Telefono));

        RuleFor(c => c.Email).EmailAddress().MaximumLength(255)
            .When(c => !string.IsNullOrWhiteSpace(c.Email));

        RuleFor(c => c.CanalPreferido).IsInEnum();
    }

    // RucValidator no distingue la estructura por fuera (solo dice si el checksum entero es válido);
    // el tercer dígito es lo único que dice qué estructura es (ver RucValidator.EsValido), así que
    // se mira aquí para decidir qué tipo de cliente puede usar ese RUC.
    private static bool EsRucDePersonaNatural(string? identificacion) =>
        identificacion is { Length: 13 } id && id.All(char.IsAsciiDigit) && id[2] - '0' <= 5;

    private static bool EsRucDeSociedad(string? identificacion) =>
        identificacion is { Length: 13 } id && id.All(char.IsAsciiDigit) && id[2] - '0' is 6 or 9;
}
