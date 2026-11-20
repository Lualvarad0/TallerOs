namespace TallerOS.Domain.Clientes;

/// <summary>Dueño del vehículo desde el punto de vista del taller (empresa_id propio). Puede existir
/// sin CuentaClienteId: ese campo solo se llena cuando la persona además se registra en Mi Garage
/// (app futura), algo que este módulo todavía no crea.</summary>
public class Cliente
{
    public Guid Id { get; private set; }
    public Guid EmpresaId { get; private set; }
    public Guid? CuentaClienteId { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public TipoCliente Tipo { get; private set; }
    public string Identificacion { get; private set; } = string.Empty;
    public string? Telefono { get; private set; }
    public string? Email { get; private set; }
    public CanalPreferido CanalPreferido { get; private set; }
    public DateTimeOffset? EliminadoEn { get; private set; }

    private Cliente() { }

    public Cliente(Guid empresaId, string nombre, TipoCliente tipo, string identificacion, string? telefono, string? email, CanalPreferido canalPreferido)
    {
        Id = Guid.CreateVersion7();
        EmpresaId = empresaId;
        Nombre = nombre;
        Tipo = tipo;
        Identificacion = identificacion;
        Telefono = telefono;
        Email = email;
        CanalPreferido = canalPreferido;
    }

    public void ActualizarDatos(string nombre, string? telefono, string? email, CanalPreferido canalPreferido)
    {
        Nombre = nombre;
        Telefono = telefono;
        Email = email;
        CanalPreferido = canalPreferido;
    }

    public void Desactivar() => EliminadoEn = DateTimeOffset.UtcNow;
}
