using FluentValidation;

namespace TallerOS.Application.Organizacion.Areas.Commands.CrearArea;

public sealed class CrearAreaCommandValidator : AbstractValidator<CrearAreaCommand>
{
    public CrearAreaCommandValidator()
    {
        RuleFor(c => c.SucursalId).NotEmpty();
        RuleFor(c => c.Nombre).NotEmpty().MaximumLength(120);
    }
}
