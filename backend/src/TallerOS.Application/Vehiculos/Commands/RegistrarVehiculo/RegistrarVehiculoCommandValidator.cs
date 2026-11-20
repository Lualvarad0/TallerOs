using FluentValidation;

namespace TallerOS.Application.Vehiculos.Commands.RegistrarVehiculo;

public sealed class RegistrarVehiculoCommandValidator : AbstractValidator<RegistrarVehiculoCommand>
{
    public RegistrarVehiculoCommandValidator()
    {
        RuleFor(c => c.ClienteId).NotEmpty();
        RuleFor(c => c.MarcaId).NotEmpty();
        RuleFor(c => c.ModeloId).NotEmpty();

        // Ni el VIN ni la placa se validan por formato aquí, a propósito (cambio de contrato del
        // 2026-11-02, ver docs/especificacion.md): un dígito verificador de VIN incorrecto ya NO
        // bloquea el registro, solo genera RegistrarVehiculoResultado.AdvertenciaVin (ver el
        // handler) — mismo trato no-bloqueante que PlacaValidator siempre tuvo (ver
        // RegistrarVehiculoCommand), porque un VIN mal transcrito a mano en recepción es un caso
        // real y bloquearlo obligaba a perder el vehículo sin poder registrarlo. Los únicos
        // bloqueos de este comando son de identidad (VIN duplicado, o VIN y placa que señalan a
        // vehículos distintos), no de formato — ver RegistrarVehiculoCommandHandler.
        RuleFor(c => c.Placa).MaximumLength(10)
            .When(c => !string.IsNullOrWhiteSpace(c.Placa));

        RuleFor(c => c)
            .Must(c => !string.IsNullOrWhiteSpace(c.Vin) || !string.IsNullOrWhiteSpace(c.Placa))
            .WithMessage("Se necesita el VIN o la placa para identificar el vehículo.");

        RuleFor(c => c.Anio).InclusiveBetween((short)1900, (short)(DateTime.UtcNow.Year + 1));

        RuleFor(c => c.Motor).MaximumLength(60)
            .When(c => !string.IsNullOrWhiteSpace(c.Motor));

        RuleFor(c => c.Rol).IsInEnum();
    }
}
