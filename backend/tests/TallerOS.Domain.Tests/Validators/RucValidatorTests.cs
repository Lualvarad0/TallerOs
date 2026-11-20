using TallerOS.Domain.Validators;

namespace TallerOS.Domain.Tests.Validators;

public sealed class RucValidatorTests
{
    [Fact]
    public void Persona_natural_valido_pasa()
    {
        // Cédula válida (1710034065, ver CedulaValidatorTests) + establecimiento 001.
        Assert.True(RucValidator.EsValido("1710034065001"));
    }

    [Fact]
    public void Persona_natural_con_establecimiento_000_no_pasa()
    {
        Assert.False(RucValidator.EsValido("1710034065000"));
    }

    [Fact]
    public void Sociedad_privada_valida_pasa()
    {
        Assert.True(RucValidator.EsValido("1791234561001"));
    }

    [Fact]
    public void Sociedad_privada_con_verificador_incorrecto_no_pasa()
    {
        Assert.False(RucValidator.EsValido("1791234562001"));
    }

    [Fact]
    public void Entidad_publica_valida_pasa()
    {
        Assert.True(RucValidator.EsValido("1761234510001"));
    }

    [Fact]
    public void Entidad_publica_con_verificador_incorrecto_no_pasa()
    {
        Assert.False(RucValidator.EsValido("1761234520001"));
    }

    [Fact]
    public void Entidad_publica_que_no_termina_en_0001_no_pasa()
    {
        Assert.False(RucValidator.EsValido("1761234510002"));
    }

    [Fact]
    public void Tercer_digito_sin_estructura_conocida_no_pasa()
    {
        Assert.False(RucValidator.EsValido("1781234561001"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("171003406500")]
    [InlineData("171003406500A")]
    public void Formato_invalido_no_pasa(string ruc)
    {
        Assert.False(RucValidator.EsValido(ruc));
    }
}
