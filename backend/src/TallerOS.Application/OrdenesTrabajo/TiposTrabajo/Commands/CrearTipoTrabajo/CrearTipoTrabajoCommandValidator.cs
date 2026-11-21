using FluentValidation;

namespace TallerOS.Application.OrdenesTrabajo.TiposTrabajo.Commands.CrearTipoTrabajo;

public sealed class CrearTipoTrabajoCommandValidator : AbstractValidator<CrearTipoTrabajoCommand>
{
    public CrearTipoTrabajoCommandValidator()
    {
        RuleFor(c => c.Nombre).NotEmpty().MaximumLength(80);
        RuleFor(c => c.TiempoEstandarMin).GreaterThan(0);
        RuleFor(c => c.IntervaloKm).GreaterThan(0).When(c => c.IntervaloKm is not null);
        RuleFor(c => c.IntervaloMeses).GreaterThan(0).When(c => c.IntervaloMeses is not null);
    }
}
