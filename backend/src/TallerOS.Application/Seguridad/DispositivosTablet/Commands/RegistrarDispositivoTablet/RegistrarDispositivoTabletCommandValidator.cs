using FluentValidation;

namespace TallerOS.Application.Seguridad.DispositivosTablet.Commands.RegistrarDispositivoTablet;

public sealed class RegistrarDispositivoTabletCommandValidator : AbstractValidator<RegistrarDispositivoTabletCommand>
{
    public RegistrarDispositivoTabletCommandValidator()
    {
        RuleFor(c => c.SucursalId).NotEmpty();
        RuleFor(c => c.Nombre).NotEmpty().MaximumLength(80);
    }
}
