using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TallerOS.Application.Comun;

namespace TallerOS.Infrastructure.Seguridad;

public sealed class TokenService(IOptions<JwtOptions> opciones) : ITokenService
{
    private readonly JwtOptions _opciones = opciones.Value;

    public string GenerarTokenPreAutenticacion(Guid usuarioId) =>
        GenerarToken(
            [
                new Claim(JwtClaims.UsuarioId, usuarioId.ToString()),
                new Claim(JwtClaims.TipoToken, JwtClaims.TipoTokenPreAuth),
            ],
            TimeSpan.FromMinutes(_opciones.MinutosExpiracionPreAuth));

    public string GenerarTokenSesion(Guid usuarioId, Guid? empresaId, bool esSuperAdmin, IReadOnlyCollection<string> permisos)
    {
        var claims = new List<Claim>
        {
            new(JwtClaims.UsuarioId, usuarioId.ToString()),
            new(JwtClaims.TipoToken, JwtClaims.TipoTokenSesion),
            new(JwtClaims.EsSuperAdmin, esSuperAdmin ? "true" : "false"),
        };

        if (empresaId is not null)
        {
            claims.Add(new Claim(JwtClaims.EmpresaId, empresaId.Value.ToString()));
        }

        // Super Admin no necesita los códigos: PermisoAuthorizationHandler lo deja pasar por el
        // claim EsSuperAdmin. Para el resto, uno por cada permiso efectivo (rol_permiso de sus roles).
        claims.AddRange(permisos.Select(codigo => new Claim(JwtClaims.Permiso, codigo)));

        return GenerarToken(claims, TimeSpan.FromMinutes(_opciones.MinutosExpiracionSesion));
    }

    private string GenerarToken(IEnumerable<Claim> claims, TimeSpan vigencia)
    {
        var credenciales = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opciones.Secret)), SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _opciones.Issuer,
            audience: _opciones.Audience,
            claims: claims,
            expires: DateTime.UtcNow.Add(vigencia),
            signingCredentials: credenciales);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
