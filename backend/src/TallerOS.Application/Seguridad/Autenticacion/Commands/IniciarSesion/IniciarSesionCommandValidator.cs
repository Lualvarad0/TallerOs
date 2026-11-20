using FluentValidation;

namespace TallerOS.Application.Seguridad.Autenticacion.Commands.IniciarSesion;

public sealed class IniciarSesionCommandValidator : AbstractValidator<IniciarSesionCommand>
{
    public IniciarSesionCommandValidator()
    {
        RuleFor(c => c.Email).NotEmpty().EmailAddress();
        RuleFor(c => c.Password).NotEmpty();
    }
}
