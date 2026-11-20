namespace TallerOS.Application.Comun;

/// <summary>Emite los dos tipos de JWT del login (ver IniciarSesionCommand/SeleccionarEmpresaCommand):
/// uno "preauth" (solo usuarioId, cuando el usuario tiene acceso a varias empresas y debe elegir)
/// y uno "sesion" (usuarioId + empresa + privilegios, el que usa el resto de la API). La validación
/// de ambos la hace el middleware JwtBearer de la Api con la misma clave/issuer/audience.</summary>
public interface ITokenService
{
    string GenerarTokenPreAutenticacion(Guid usuarioId);

    string GenerarTokenSesion(Guid usuarioId, Guid? empresaId, bool esSuperAdmin, IReadOnlyCollection<string> permisos);
}
