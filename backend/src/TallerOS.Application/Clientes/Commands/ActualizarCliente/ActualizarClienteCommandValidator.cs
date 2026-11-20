using FluentValidation;
using TallerOS.Domain.Validators;

namespace TallerOS.Application.Clientes.Commands.ActualizarCliente;

public sealed class ActualizarClienteCommandValidator : AbstractValidator<ActualizarClienteCommand>
{
    public ActualizarClienteCommandValidator()
    {
        RuleFor(c => c.ClienteId).NotEmpty();
        RuleFor(c => c.Nombre).NotEmpty().MaximumLength(160);

        RuleFor(c => c.Telefono).Must(TelefonoValidator.EsValido)
            .WithMessage("El teléfono no es válido.")
            .When(c => !string.IsNullOrWhiteSpace(c.Telefono));

        RuleFor(c => c.Email).EmailAddress().MaximumLength(255)
            .When(c => !string.IsNullOrWhiteSpace(c.Email));

        RuleFor(c => c.CanalPreferido).IsInEnum();
    }
}
