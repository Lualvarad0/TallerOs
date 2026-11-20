using FluentValidation;

namespace TallerOS.Application.Organizacion.Empresas.Commands.CrearEmpresa;

public sealed class CrearEmpresaCommandValidator : AbstractValidator<CrearEmpresaCommand>
{
    public CrearEmpresaCommandValidator()
    {
        RuleFor(c => c.Nombre).NotEmpty().MaximumLength(160);

        // Solo formato (13 dígitos). El dígito verificador real de cédula/RUC lo porta el Módulo 2
        // (Catálogos y validadores); no hay que adelantar ese trabajo aquí.
        RuleFor(c => c.Ruc).NotEmpty().Matches(@"^\d{13}$").WithMessage("El RUC debe tener 13 dígitos.");

        RuleFor(c => c.PlanId).NotEmpty();

        RuleFor(c => c.EmailDueño).NotEmpty().EmailAddress().MaximumLength(255);
        RuleFor(c => c.PasswordDueño).NotEmpty().MinimumLength(8)
            .WithMessage("La contraseña debe tener al menos 8 caracteres.");
    }
}
