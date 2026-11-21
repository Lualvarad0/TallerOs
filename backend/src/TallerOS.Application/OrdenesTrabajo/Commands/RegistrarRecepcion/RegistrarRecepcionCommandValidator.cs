using FluentValidation;

namespace TallerOS.Application.OrdenesTrabajo.Commands.RegistrarRecepcion;

public sealed class RegistrarRecepcionCommandValidator : AbstractValidator<RegistrarRecepcionCommand>
{
    public RegistrarRecepcionCommandValidator()
    {
        RuleFor(c => c.SucursalId).NotEmpty();
        RuleFor(c => c.VehiculoId).NotEmpty();
        RuleFor(c => c.ClienteId).NotEmpty();
        RuleFor(c => c.Prioridad).IsInEnum();
        RuleFor(c => c.KmIngreso).GreaterThanOrEqualTo(0);
        RuleFor(c => c.Combustible).InclusiveBetween((short)0, (short)8);
        RuleFor(c => c.UsuarioId).NotEmpty();

        RuleForEach(c => c.DaniosPrevios).ChildRules(danio =>
        {
            danio.RuleFor(d => d.Zona).NotEmpty().MaximumLength(40);
            danio.RuleFor(d => d.Tipo).IsInEnum();
        });
    }
}
