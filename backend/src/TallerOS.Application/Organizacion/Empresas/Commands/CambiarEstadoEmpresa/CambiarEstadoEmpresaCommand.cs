using TallerOS.Application.Comun.Cqrs;
using TallerOS.Domain.Organizacion;

namespace TallerOS.Application.Organizacion.Empresas.Commands.CambiarEstadoEmpresa;

public sealed record CambiarEstadoEmpresaCommand(Guid EmpresaId, EstadoEmpresa NuevoEstado) : IRequest<Unit>;
