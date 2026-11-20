using FluentValidation;

namespace TallerOS.Application.Organizacion.Personal.Commands.CrearPersonal;

public sealed class CrearPersonalCommandValidator : AbstractValidator<CrearPersonalCommand>
{
    public CrearPersonalCommandValidator()
    {
        RuleFor(c => c.AreaId).NotEmpty();
        RuleFor(c => c.Nombre).NotEmpty().MaximumLength(160);
        RuleFor(c => c.TarifaHora).GreaterThanOrEqualTo(0).When(c => c.TarifaHora.HasValue);

        // Con acceso: inicia sesión con cuenta propia, no con PIN. Sin acceso: PIN para fichar en
        // la tablet compartida (ver DispositivoTablet), nunca cuenta propia.
        RuleFor(c => c.UsuarioId).Empty().When(c => !c.TieneAcceso)
            .WithMessage("Personal sin acceso no debe tener un usuario asociado.");
        RuleFor(c => c.Pin).Empty().When(c => c.TieneAcceso)
            .WithMessage("Personal con acceso inicia sesión con su cuenta, no con PIN.");
        RuleFor(c => c.Pin).Matches(@"^\d{4,6}$").When(c => !c.TieneAcceso && c.Pin != null)
            .WithMessage("El PIN debe tener entre 4 y 6 dígitos.");
    }
}
