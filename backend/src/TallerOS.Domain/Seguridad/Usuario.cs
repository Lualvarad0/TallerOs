namespace TallerOS.Domain.Seguridad;

/// <summary>Global: una cuenta puede tener acceso a varias empresas vía UsuarioRol.</summary>
public class Usuario
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public bool Activo { get; private set; } = true;

    /// <summary>Acceso de plataforma (todas las empresas), no de un rol de empresa. Ver política RLS de bypass.</summary>
    public bool EsSuperAdmin { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    private Usuario() { }

    public Usuario(string email, string passwordHash, bool esSuperAdmin = false)
    {
        Id = Guid.CreateVersion7();
        Email = email;
        PasswordHash = passwordHash;
        Activo = true;
        EsSuperAdmin = esSuperAdmin;
        CreadoEn = DateTimeOffset.UtcNow;
    }
}
