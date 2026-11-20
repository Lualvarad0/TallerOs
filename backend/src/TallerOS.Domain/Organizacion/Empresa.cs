namespace TallerOS.Domain.Organizacion;

public class Empresa
{
    public Guid Id { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public string Ruc { get; private set; } = string.Empty;
    public EstadoEmpresa Estado { get; private set; }
    public DateTimeOffset CreadoEn { get; private set; }
    public DateTimeOffset? EliminadoEn { get; private set; }

    private Empresa() { }

    public Empresa(string nombre, string ruc)
    {
        Id = Guid.CreateVersion7();
        Nombre = nombre;
        Ruc = ruc;
        Estado = EstadoEmpresa.Activa;
        CreadoEn = DateTimeOffset.UtcNow;
    }

    public void ActualizarDatos(string nombre, string ruc)
    {
        Nombre = nombre;
        Ruc = ruc;
    }

    public void CambiarEstado(EstadoEmpresa nuevoEstado)
    {
        Estado = nuevoEstado;
    }
}
