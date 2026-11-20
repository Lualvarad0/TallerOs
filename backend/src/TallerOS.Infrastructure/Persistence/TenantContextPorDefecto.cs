using TallerOS.Application.Comun;

namespace TallerOS.Infrastructure.Persistence;

/// <summary>Sin empresa y sin privilegios: con RLS activo, cualquier tabla de empresa queda vacía
/// para esta sesión. Seguro por defecto mientras el paso 3 no reemplace esto por el contexto real
/// (leído de los claims del JWT en cada request).</summary>
public sealed class TenantContextPorDefecto : ITenantContext
{
    public Guid? EmpresaId => null;
    public bool EsSuperAdmin => false;
}
