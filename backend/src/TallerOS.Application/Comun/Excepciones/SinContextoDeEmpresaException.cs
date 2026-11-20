namespace TallerOS.Application.Comun.Excepciones;

/// <summary>El request llegó autenticado pero sin una empresa seleccionada (ver flujo de login:
/// IniciarSesion -> SeleccionarEmpresa). La Api la traduce a 403. Casi todos los Commands de
/// Organización/Seguridad la necesitan, porque toman el empresa_id de ITenantContext, nunca del
/// cliente (evita que alguien mande el empresa_id de otra empresa en el body).</summary>
public sealed class SinContextoDeEmpresaException() : Exception("La sesión no tiene una empresa seleccionada.");
