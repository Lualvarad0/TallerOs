namespace TallerOS.Domain.Seguridad;

/// <summary>Tablet compartida (recepción, taller) donde varios Personal fichan con su PIN.
/// El PIN identifica a la persona (Personal.PinHash); este token identifica y autoriza al dispositivo,
/// para que no cualquier dispositivo pueda probar PINs contra la API.</summary>
public class DispositivoTablet
{
    public Guid Id { get; private set; }
    public Guid EmpresaId { get; private set; }
    public Guid SucursalId { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public string TokenHash { get; private set; } = string.Empty;
    public bool Activo { get; private set; } = true;
    public DateTimeOffset CreadoEn { get; private set; }
    public DateTimeOffset? UltimoUsoEn { get; private set; }

    private DispositivoTablet() { }

    public DispositivoTablet(Guid empresaId, Guid sucursalId, string nombre, string tokenHash)
    {
        Id = Guid.CreateVersion7();
        EmpresaId = empresaId;
        SucursalId = sucursalId;
        Nombre = nombre;
        TokenHash = tokenHash;
        Activo = true;
        CreadoEn = DateTimeOffset.UtcNow;
    }

    public void Revocar() => Activo = false;

    public void RegistrarUso(DateTimeOffset momento) => UltimoUsoEn = momento;
}
