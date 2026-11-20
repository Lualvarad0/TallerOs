using TallerOS.Application.Comun.Cqrs;

namespace TallerOS.Application.Organizacion.Empresas.Commands.CrearEmpresa;

/// <summary>Alta de una empresa nueva (registro de un taller en TallerOS): crea la empresa, su
/// suscripción, un rol "Dueño/Gerente" con todos los permisos existentes, y la cuenta de usuario
/// del dueño ya vinculada a ese rol para toda la empresa. Sucursales, áreas y personal se crean
/// después, ya autenticado como ese dueño.</summary>
public sealed record CrearEmpresaCommand(
    string Nombre,
    string Ruc,
    Guid PlanId,
    string EmailDueño,
    string PasswordDueño) : IRequest<CrearEmpresaResultado>;

public sealed record CrearEmpresaResultado(Guid EmpresaId, Guid UsuarioId);
