using FluentValidation;

namespace TallerOS.Application.OrdenesTrabajo.Subtareas.Commands.CrearSubtarea;

public sealed class CrearSubtareaCommandValidator : AbstractValidator<CrearSubtareaCommand>
{
    public CrearSubtareaCommandValidator()
    {
        RuleFor(c => c.OtId).NotEmpty();
        RuleFor(c => c.TipoTrabajoId).NotEmpty();
        RuleFor(c => c.TiempoEstimadoMin).GreaterThan(0);
    }
}
