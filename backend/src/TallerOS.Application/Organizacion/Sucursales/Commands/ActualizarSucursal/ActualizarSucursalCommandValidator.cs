using FluentValidation;

namespace TallerOS.Application.Organizacion.Sucursales.Commands.ActualizarSucursal;

public sealed class ActualizarSucursalCommandValidator : AbstractValidator<ActualizarSucursalCommand>
{
    public ActualizarSucursalCommandValidator()
    {
        RuleFor(c => c.SucursalId).NotEmpty();
        RuleFor(c => c.Nombre).NotEmpty().MaximumLength(160);
        RuleFor(c => c.Direccion).MaximumLength(240);
    }
}
