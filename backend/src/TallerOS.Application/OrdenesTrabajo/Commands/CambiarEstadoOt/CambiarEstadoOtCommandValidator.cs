using FluentValidation;

namespace TallerOS.Application.OrdenesTrabajo.Commands.CambiarEstadoOt;

public sealed class CambiarEstadoOtCommandValidator : AbstractValidator<CambiarEstadoOtCommand>
{
    public CambiarEstadoOtCommandValidator()
    {
        RuleFor(c => c.OtId).NotEmpty();
        RuleFor(c => c.NuevoEstadoId).NotEmpty();
        RuleFor(c => c.UsuarioId).NotEmpty();
    }
}
