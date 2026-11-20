using FluentValidation;

namespace TallerOS.Application.Seguridad.UsuariosRoles.Commands.InvitarUsuario;

public sealed class InvitarUsuarioCommandValidator : AbstractValidator<InvitarUsuarioCommand>
{
    public InvitarUsuarioCommandValidator()
    {
        RuleFor(c => c.Email).NotEmpty().EmailAddress().MaximumLength(255);
        RuleFor(c => c.Password).MinimumLength(8).When(c => c.Password != null)
            .WithMessage("La contraseña debe tener al menos 8 caracteres.");
        RuleFor(c => c.RolId).NotEmpty();
    }
}
