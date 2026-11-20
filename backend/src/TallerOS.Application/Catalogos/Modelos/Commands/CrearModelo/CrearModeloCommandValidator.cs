using FluentValidation;

namespace TallerOS.Application.Catalogos.Modelos.Commands.CrearModelo;

public sealed class CrearModeloCommandValidator : AbstractValidator<CrearModeloCommand>
{
    public CrearModeloCommandValidator()
    {
        RuleFor(c => c.MarcaId).NotEmpty();
        RuleFor(c => c.Nombre).NotEmpty().MaximumLength(100);
    }
}
