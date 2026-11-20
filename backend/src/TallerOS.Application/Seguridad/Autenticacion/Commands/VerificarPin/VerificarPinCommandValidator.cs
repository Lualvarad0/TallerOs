using FluentValidation;

namespace TallerOS.Application.Seguridad.Autenticacion.Commands.VerificarPin;

public sealed class VerificarPinCommandValidator : AbstractValidator<VerificarPinCommand>
{
    public VerificarPinCommandValidator()
    {
        RuleFor(c => c.TokenDispositivo).NotEmpty();
        RuleFor(c => c.Pin).NotEmpty();
    }
}
