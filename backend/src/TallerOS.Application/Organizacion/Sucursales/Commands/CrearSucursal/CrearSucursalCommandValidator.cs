using FluentValidation;

namespace TallerOS.Application.Organizacion.Sucursales.Commands.CrearSucursal;

public sealed class CrearSucursalCommandValidator : AbstractValidator<CrearSucursalCommand>
{
    public CrearSucursalCommandValidator()
    {
        RuleFor(c => c.Nombre).NotEmpty().MaximumLength(160);
        RuleFor(c => c.Direccion).MaximumLength(240);
    }
}
