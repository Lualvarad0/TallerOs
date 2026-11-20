using FluentValidation;

namespace TallerOS.Application.Seguridad.Roles.Commands.AsignarPermisos;

public sealed class AsignarPermisosCommandValidator : AbstractValidator<AsignarPermisosCommand>
{
    public AsignarPermisosCommandValidator()
    {
        RuleFor(c => c.RolId).NotEmpty();
        RuleForEach(c => c.Codigos).NotEmpty();
    }
}
