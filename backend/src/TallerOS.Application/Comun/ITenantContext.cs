namespace TallerOS.Application.Comun;

/// <summary>Empresa y privilegios de la sesión actual. El filtro global de EF Core y la variable de
/// sesión de PostgreSQL para RLS (ver ITenantSessionConfigurator en Infrastructure) leen de aquí.
/// La implementación real (a partir de claims del JWT) se agrega en el paso 3 (backend/autenticación);
/// por ahora Infrastructure registra una implementación por defecto sin empresa ni privilegios.</summary>
public interface ITenantContext
{
    Guid? EmpresaId { get; }
    bool EsSuperAdmin { get; }
}
