using FluentValidation;

namespace TallerOS.Application.Catalogos.Marcas.Commands.CrearMarca;

public sealed class CrearMarcaCommandValidator : AbstractValidator<CrearMarcaCommand>
{
    public CrearMarcaCommandValidator()
    {
        RuleFor(c => c.TipoVehiculoId).GreaterThan((short)0);
        RuleFor(c => c.Nombre).NotEmpty().MaximumLength(80);
    }
}
