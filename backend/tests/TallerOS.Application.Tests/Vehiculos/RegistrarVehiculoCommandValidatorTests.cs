using TallerOS.Application.Vehiculos.Commands.RegistrarVehiculo;
using TallerOS.Domain.Vehiculos;

namespace TallerOS.Application.Tests.Vehiculos;

public sealed class RegistrarVehiculoCommandValidatorTests
{
    private readonly RegistrarVehiculoCommandValidator _validator = new();

    private static RegistrarVehiculoCommand Comando(string? vin = "1HGCM82633A004352", string? placa = "ABC-1234") =>
        new(Guid.NewGuid(), vin, placa, Guid.NewGuid(), Guid.NewGuid(), 2020, "2.0L", RolClienteVehiculo.Propietario);

    [Fact]
    public void Comando_con_vin_valido_y_placa_pasa()
    {
        var resultado = _validator.Validate(Comando());

        Assert.True(resultado.IsValid);
    }

    [Fact]
    public void Comando_sin_vin_pero_con_placa_pasa()
    {
        // Un VIN ilegible es real (ver Vehiculo): la placa sola basta para identificar el vehículo.
        var resultado = _validator.Validate(Comando(vin: null));

        Assert.True(resultado.IsValid);
    }

    [Fact]
    public void Comando_sin_vin_ni_placa_no_pasa()
    {
        var resultado = _validator.Validate(Comando(vin: null, placa: null));

        Assert.False(resultado.IsValid);
    }

    [Fact]
    public void Vin_con_digito_verificador_incorrecto_pasa_igual()
    {
        // Cambio de contrato del 2026-11-02 (ver docs/especificacion.md): un VIN con dígito
        // verificador incorrecto ya no bloquea el registro, solo genera
        // RegistrarVehiculoResultado.AdvertenciaVin (ver RegistrarVehiculoCommandHandler) — mismo
        // trato no-bloqueante que ya tenía la placa, ver la prueba de abajo.
        var resultado = _validator.Validate(Comando(vin: "1HGCM82633A004353"));

        Assert.True(resultado.IsValid);
    }

    [Fact]
    public void Placa_con_formato_no_reconocido_pasa_igual()
    {
        // PlacaValidator es una advertencia de UI, no un bloqueo (ver RegistrarVehiculoCommand):
        // hay placas provisionales y el formato cambia con el tiempo.
        var resultado = _validator.Validate(Comando(placa: "PROVISIONAL-1"[..10]));

        Assert.True(resultado.IsValid);
    }

    [Fact]
    public void Anio_fuera_de_rango_no_pasa()
    {
        var resultado = _validator.Validate(Comando() with { Anio = 1800 });

        Assert.False(resultado.IsValid);
    }
}
