using FluentValidation;

namespace TallerOS.Application.OrdenesTrabajo.Subtareas.Commands.ActualizarSubtarea;

public sealed class ActualizarSubtareaCommandValidator : AbstractValidator<ActualizarSubtareaCommand>
{
    public ActualizarSubtareaCommandValidator()
    {
        RuleFor(c => c.SubtareaId).NotEmpty();
        RuleFor(c => c.Estado).IsInEnum();
    }
}
