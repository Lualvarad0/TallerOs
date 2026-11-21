namespace TallerOS.Domain.OrdenesTrabajo;

/// <summary>Datos capturados al recibir el vehículo. Relación 1:1 con OrdenTrabajo: nace junto con
/// ella y no se crea por separado.</summary>
public class Recepcion
{
    public Guid Id { get; private set; }
    public Guid EmpresaId { get; private set; }
    public Guid OtId { get; private set; }

    /// <summary>Octavos de tanque (0 a 8, como el medidor del vehículo).</summary>
    public short Combustible { get; private set; }

    public string? Objetos { get; private set; }
    public string? Observaciones { get; private set; }

    /// <summary>Nula hasta que el cliente firma; no pasa por la tabla `adjunto` porque es un dato
    /// único y propio de la recepción, no una foto más entre varias.</summary>
    public string? FirmaUrl { get; private set; }

    private Recepcion() { }

    public Recepcion(Guid empresaId, Guid otId, short combustible, string? objetos, string? observaciones)
    {
        Id = Guid.CreateVersion7();
        EmpresaId = empresaId;
        OtId = otId;
        Combustible = combustible;
        Objetos = objetos;
        Observaciones = observaciones;
    }

    public void RegistrarFirma(string firmaUrl) => FirmaUrl = firmaUrl;
}
