namespace TallerOS.Domain.OrdenesTrabajo;

/// <summary>Archivo subido a almacenamiento S3 (Garage en local), asociado por entidad+id de forma
/// genérica a propósito: hoy solo Recepción lo usa, pero Hallazgo, TrabajoTerminado y Factura (de
/// los módulos 6 y 7) reutilizarán la misma tabla en vez de duplicarla. Sin llave foránea real hacia
/// esas entidades porque varían según `Tipo`; el caso de uso es responsable de no referenciar un id
/// que no existe.</summary>
public class Adjunto
{
    public Guid Id { get; private set; }
    public Guid EmpresaId { get; private set; }
    public string Entidad { get; private set; } = string.Empty;
    public Guid EntidadId { get; private set; }
    public TipoAdjunto Tipo { get; private set; }
    public string Url { get; private set; } = string.Empty;
    public string? MiniaturaUrl { get; private set; }
    public Guid AutorUsuarioId { get; private set; }
    public DateTimeOffset CreadoEn { get; private set; }

    private Adjunto() { }

    public Adjunto(Guid empresaId, string entidad, Guid entidadId, TipoAdjunto tipo, string url, string? miniaturaUrl, Guid autorUsuarioId)
    {
        Id = Guid.CreateVersion7();
        EmpresaId = empresaId;
        Entidad = entidad;
        EntidadId = entidadId;
        Tipo = tipo;
        Url = url;
        MiniaturaUrl = miniaturaUrl;
        AutorUsuarioId = autorUsuarioId;
        CreadoEn = DateTimeOffset.UtcNow;
    }
}
