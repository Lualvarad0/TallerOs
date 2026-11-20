namespace TallerOS.Domain.Catalogos;

/// <summary>Catálogo global (sin empresa_id, ver docs/convenciones.md § Seguridad). Ligada a un tipo_vehiculo
/// porque una misma marca comercial puede fabricar más de un tipo (ej. Honda autos y Honda motos
/// son filas distintas, cada una con su propio conjunto de modelos).</summary>
public class Marca
{
    public Guid Id { get; private set; }
    public short TipoVehiculoId { get; private set; }
    public string Nombre { get; private set; } = string.Empty;

    private Marca() { }

    public Marca(short tipoVehiculoId, string nombre)
    {
        Id = Guid.CreateVersion7();
        TipoVehiculoId = tipoVehiculoId;
        Nombre = nombre;
    }
}
