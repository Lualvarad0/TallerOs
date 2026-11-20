using FluentValidation;

namespace TallerOS.Application.Organizacion.Empresas.Commands.ActualizarEmpresa;

public sealed class ActualizarEmpresaCommandValidator : AbstractValidator<ActualizarEmpresaCommand>
{
    public ActualizarEmpresaCommandValidator()
    {
        RuleFor(c => c.EmpresaId).NotEmpty();
        RuleFor(c => c.Nombre).NotEmpty().MaximumLength(160);
        RuleFor(c => c.Ruc).NotEmpty().Matches(@"^\d{13}$").WithMessage("El RUC debe tener 13 dígitos.");
    }
}
