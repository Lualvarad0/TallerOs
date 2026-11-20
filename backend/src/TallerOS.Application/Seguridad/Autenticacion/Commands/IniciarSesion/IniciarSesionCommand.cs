using TallerOS.Application.Comun.Cqrs;

namespace TallerOS.Application.Seguridad.Autenticacion.Commands.IniciarSesion;

public sealed record IniciarSesionCommand(string Email, string Password) : IRequest<IniciarSesionResultado>;

public sealed record EmpresaAccesibleDto(Guid EmpresaId, string NombreEmpresa);

/// <summary>Exactamente uno de los dos tokens viene con valor, según cuántas empresas tenga el usuario:
/// TokenSesion si es Super Admin o tiene una sola empresa (ya no hace falta elegir); TokenPreAutenticacion
/// + Empresas si tiene varias (el cliente llama a SeleccionarEmpresaCommand con uno de esos ids).</summary>
public sealed record IniciarSesionResultado(string? TokenSesion, string? TokenPreAutenticacion, IReadOnlyList<EmpresaAccesibleDto> Empresas);
