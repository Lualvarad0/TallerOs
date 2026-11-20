namespace TallerOS.Domain.Clientes;

/// <summary>Catálogo global (sin empresa_id, ver docs/convenciones.md § Seguridad): la cuenta de Mi Garage,
/// compartida entre todas las empresas que atendieron alguna vez al mismo vehículo. Nada en este
/// módulo crea filas aquí todavía (no existe el registro de Mi Garage); la tabla ya existe para que
/// vehiculo_propietario y cliente.cuenta_cliente_id tengan dónde apuntar cuando ese paso se construya.</summary>
public class CuentaCliente
{
    public Guid Id { get; private set; }
    public Guid UsuarioId { get; private set; }
    public string Telefono { get; private set; } = string.Empty;
    public bool TelefonoVerificado { get; private set; }

    private CuentaCliente() { }

    public CuentaCliente(Guid usuarioId, string telefono)
    {
        Id = Guid.CreateVersion7();
        UsuarioId = usuarioId;
        Telefono = telefono;
        TelefonoVerificado = false;
    }
}
