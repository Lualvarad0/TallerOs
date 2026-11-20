using TallerOS.Application.Comun.Cqrs;

namespace TallerOS.Application.Organizacion.Empresas.Commands.ActualizarEmpresa;

public sealed record ActualizarEmpresaCommand(Guid EmpresaId, string Nombre, string Ruc) : IRequest<Unit>;
