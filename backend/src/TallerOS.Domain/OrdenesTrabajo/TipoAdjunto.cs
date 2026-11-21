namespace TallerOS.Domain.OrdenesTrabajo;

/// <summary>A qué entidad pertenece el archivo subido. Hoy solo Recepción lo usa; Hallazgo,
/// TrabajoTerminado y Factura están reservados para los módulos 6 y 7, que reutilizarán esta misma
/// tabla en vez de crear una propia.</summary>
public enum TipoAdjunto
{
    Recepcion,
    Hallazgo,
    TrabajoTerminado,
    Factura,
}
