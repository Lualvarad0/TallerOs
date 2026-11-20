using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using TallerOS.Application.Comun;

namespace TallerOS.Infrastructure.Seguridad;

/// <summary>PBKDF2 vía PasswordHasher de ASP.NET Core Identity (Microsoft.Extensions.Identity.Core):
/// gratis y bien probado, sin el problema de licencia que tienen algunas librerías de bcrypt.
/// El "TUser" genérico de PasswordHasher no se usa para nada del hash en sí, es solo de la firma.</summary>
public sealed class HashService : IHashService
{
    private readonly PasswordHasher<object> _hasher = new();

    public string Hash(string valor) => _hasher.HashPassword(this, valor);

    public bool Verificar(string valor, string hash) =>
        _hasher.VerifyHashedPassword(this, hash, valor) != PasswordVerificationResult.Failed;

    public string HashDeterministico(string valor) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(valor)));
}
