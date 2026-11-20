namespace TallerOS.Infrastructure.Seguridad;

/// <summary>Bindeado desde la sección "Jwt" de la configuración. Secret viene de .env
/// (Jwt__Secret), nunca de appsettings.json.</summary>
public sealed class JwtOptions
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int MinutosExpiracionPreAuth { get; set; } = 10;
    public int MinutosExpiracionSesion { get; set; } = 480;
}
