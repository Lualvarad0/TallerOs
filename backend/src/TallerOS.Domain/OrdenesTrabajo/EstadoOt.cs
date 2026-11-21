namespace TallerOS.Domain.OrdenesTrabajo;

/// <summary>Un paso del flujo de la OT, configurable por empresa (no es un enum fijo). Hoy se
/// siembran 8 estados estándar al crear la empresa (ver CrearEmpresaCommandHandler) y no hay
/// pantalla para editarlos; ese CRUD queda para cuando exista Configuración.</summary>
public class EstadoOt
{
    public Guid Id { get; private set; }
    public Guid EmpresaId { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public short Orden { get; private set; }
    public bool EsFinal { get; private set; }

    /// <summary>Token de color del design system (ver docs/diseno/README.md § Mapeo de estados de
    /// OT → color) para pintar la columna del Kanban y el borde de la tarjeta.</summary>
    public string ColorToken { get; private set; } = string.Empty;

    private EstadoOt() { }

    public EstadoOt(Guid empresaId, string nombre, short orden, bool esFinal, string colorToken)
    {
        Id = Guid.CreateVersion7();
        EmpresaId = empresaId;
        Nombre = nombre;
        Orden = orden;
        EsFinal = esFinal;
        ColorToken = colorToken;
    }
}
