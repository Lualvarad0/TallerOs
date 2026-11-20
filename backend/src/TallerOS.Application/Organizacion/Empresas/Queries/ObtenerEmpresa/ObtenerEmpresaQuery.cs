using TallerOS.Application.Comun.Cqrs;

namespace TallerOS.Application.Organizacion.Empresas.Queries.ObtenerEmpresa;

public sealed record ObtenerEmpresaQuery(Guid EmpresaId) : IRequest<EmpresaDto>;
