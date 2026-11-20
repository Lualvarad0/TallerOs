namespace TallerOS.Domain.Organizacion;

public class Personal
{
    public Guid Id { get; private set; }
    public Guid EmpresaId { get; private set; }
    public Guid AreaId { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public decimal? TarifaHora { get; private set; }

    /// <summary>Si es false, no inicia sesión por su cuenta: otra persona registra su trabajo.</summary>
    public bool TieneAcceso { get; private set; }

    /// <summary>Nulo cuando TieneAcceso es false (personal sin cuenta propia).</summary>
    public Guid? UsuarioId { get; private set; }

    /// <summary>Hash del PIN para fichar en una tablet compartida (ver DispositivoTablet). Nulo si no usa PIN.</summary>
    public string? PinHash { get; private set; }

    public DateTimeOffset? EliminadoEn { get; private set; }

    private Personal() { }

    public Personal(Guid empresaId, Guid areaId, string nombre, decimal? tarifaHora, bool tieneAcceso, Guid? usuarioId, string? pinHash)
    {
        Id = Guid.CreateVersion7();
        EmpresaId = empresaId;
        AreaId = areaId;
        Nombre = nombre;
        TarifaHora = tarifaHora;
        TieneAcceso = tieneAcceso;
        UsuarioId = usuarioId;
        PinHash = pinHash;
    }

    public void ActualizarDatos(Guid areaId, string nombre, decimal? tarifaHora)
    {
        AreaId = areaId;
        Nombre = nombre;
        TarifaHora = tarifaHora;
    }

    public void AsignarPin(string pinHash) => PinHash = pinHash;

    public void Desactivar() => EliminadoEn = DateTimeOffset.UtcNow;
}
