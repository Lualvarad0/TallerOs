using TallerOS.Domain.Seguridad;

namespace TallerOS.Domain.Tests.Seguridad;

public sealed class DispositivoTabletTests
{
    [Fact]
    public void El_constructor_arranca_activo_sin_uso_previo()
    {
        var dispositivo = new DispositivoTablet(Guid.NewGuid(), Guid.NewGuid(), "Tablet recepción", "hash-de-prueba");

        Assert.True(dispositivo.Activo);
        Assert.Null(dispositivo.UltimoUsoEn);
    }

    [Fact]
    public void Revocar_desactiva_el_dispositivo()
    {
        var dispositivo = new DispositivoTablet(Guid.NewGuid(), Guid.NewGuid(), "Tablet recepción", "hash-de-prueba");

        dispositivo.Revocar();

        Assert.False(dispositivo.Activo);
    }

    [Fact]
    public void RegistrarUso_guarda_el_momento_indicado()
    {
        var dispositivo = new DispositivoTablet(Guid.NewGuid(), Guid.NewGuid(), "Tablet recepción", "hash-de-prueba");
        var momento = new DateTimeOffset(2026, 10, 31, 8, 0, 0, TimeSpan.Zero);

        dispositivo.RegistrarUso(momento);

        Assert.Equal(momento, dispositivo.UltimoUsoEn);
    }
}
