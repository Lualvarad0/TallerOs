using Microsoft.EntityFrameworkCore;
using TallerOS.Application.Comun;
using TallerOS.Application.Comun.Cqrs;

namespace TallerOS.Application.Clientes.Queries.ListarClientes;

public sealed class ListarClientesQueryHandler(IAppDbContext db) : IRequestHandler<ListarClientesQuery, IReadOnlyList<ClienteDto>>
{
    public async Task<IReadOnlyList<ClienteDto>> Handle(ListarClientesQuery request, CancellationToken cancellationToken)
    {
        // El filtro global por empresa_id ya acota esto a la empresa de la sesión.
        var consulta = db.Clientes.Where(c => c.EliminadoEn == null);

        if (!string.IsNullOrWhiteSpace(request.Busqueda))
        {
            // Contains sobre ToUpper(), no EF.Functions.ILike: ILike es de Npgsql, y Application no
            // referencia ese paquete a propósito (ver IAppDbContext, para no acoplarse a un motor
            // concreto). Por eso esta búsqueda NO aprovecha los índices gin_trgm_ops de
            // ClienteConfiguration (esos sí son para ILike); son pocos clientes por empresa, así que
            // el costo es aceptable por ahora. Mismo campo que especificación.md pide para recepción
            // ("por placa, cédula o nombre"), aquí sin la parte de placa porque esta consulta es la
            // ficha de clientes, no la de recepción (otro módulo).
            var terminoBusqueda = request.Busqueda.ToUpperInvariant();
            consulta = consulta.Where(c =>
                c.Nombre.ToUpper().Contains(terminoBusqueda) || c.Identificacion.ToUpper().Contains(terminoBusqueda));
        }

        return await consulta
            .OrderBy(c => c.Nombre)
            .Select(c => new ClienteDto(c.Id, c.Nombre, c.Tipo, c.Identificacion, c.Telefono, c.Email, c.CanalPreferido))
            .ToListAsync(cancellationToken);
    }
}
