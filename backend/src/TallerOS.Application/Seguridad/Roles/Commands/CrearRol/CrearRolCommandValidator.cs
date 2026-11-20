using FluentValidation;

namespace TallerOS.Application.Seguridad.Roles.Commands.CrearRol;

public sealed class CrearRolCommandValidator : AbstractValidator<CrearRolCommand>
{
    public CrearRolCommandValidator()
    {
        RuleFor(c => c.Nombre).NotEmpty().MaximumLength(80);
    }
}
