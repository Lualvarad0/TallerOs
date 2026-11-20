namespace TallerOS.Domain.Plataforma;

public class Plan
{
    public Guid Id { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public int? LimiteUsuarios { get; private set; }
    public int? LimiteSucursales { get; private set; }
    public bool PermiteWhatsAppOficial { get; private set; }
    public bool Activo { get; private set; } = true;

    private Plan() { }

    public Plan(string nombre, int? limiteUsuarios, int? limiteSucursales, bool permiteWhatsAppOficial)
    {
        Id = Guid.CreateVersion7();
        Nombre = nombre;
        LimiteUsuarios = limiteUsuarios;
        LimiteSucursales = limiteSucursales;
        PermiteWhatsAppOficial = permiteWhatsAppOficial;
        Activo = true;
    }
}
