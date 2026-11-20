using FluentValidation;

namespace TallerOS.Application.Seguridad.Autenticacion.Commands.SeleccionarEmpresa;

public sealed class SeleccionarEmpresaCommandValidator : AbstractValidator<SeleccionarEmpresaCommand>
{
    public SeleccionarEmpresaCommandValidator()
    {
        RuleFor(c => c.UsuarioId).NotEmpty();
        RuleFor(c => c.EmpresaId).NotEmpty();
    }
}
