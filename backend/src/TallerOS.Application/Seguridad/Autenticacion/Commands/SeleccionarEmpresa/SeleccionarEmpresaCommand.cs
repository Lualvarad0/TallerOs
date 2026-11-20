using TallerOS.Application.Comun.Cqrs;

namespace TallerOS.Application.Seguridad.Autenticacion.Commands.SeleccionarEmpresa;

/// <summary>UsuarioId viene de los claims del token pre-auth ya validado (lo pone el controller de
/// la Api, nunca el cliente); EmpresaId sí lo manda el cliente, y el handler verifica que ese usuario
/// realmente tenga acceso a ella antes de emitir el token de sesión.</summary>
public sealed record SeleccionarEmpresaCommand(Guid UsuarioId, Guid EmpresaId) : IRequest<SeleccionarEmpresaResultado>;

public sealed record SeleccionarEmpresaResultado(string TokenSesion);
