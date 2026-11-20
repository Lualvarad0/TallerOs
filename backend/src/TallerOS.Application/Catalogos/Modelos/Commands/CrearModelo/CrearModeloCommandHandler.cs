using Microsoft.EntityFrameworkCore;
using TallerOS.Application.Catalogos;
using TallerOS.Application.Comun;
using TallerOS.Application.Comun.Cqrs;
using TallerOS.Application.Comun.Excepciones;
using TallerOS.Domain.Catalogos;

namespace TallerOS.Application.Catalogos.Modelos.Commands.CrearModelo;

public sealed class CrearModeloCommandHandler(IAppDbContext db, ICatalogoSimilitudService similitud)
    : IRequestHandler<CrearModeloCommand, Guid>
{
    public async Task<Guid> Handle(CrearModeloCommand request, CancellationToken cancellationToken)
    {
        var existeMarca = await db.Marcas.AnyAsync(m => m.Id == request.MarcaId, cancellationToken);
        if (!existeMarca)
        {
            throw new EntidadNoEncontradaException("Marca", request.MarcaId);
        }

        var parecidos = await similitud.BuscarModelosParecidosAsync(request.MarcaId, request.Nombre, cancellationToken);
        if (parecidos.Count > 0)
        {
            throw new ConflictoDeNegocioException(
                $"Ya existe un modelo parecido: {string.Join(", ", parecidos)}. Si es un modelo distinto, usa un nombre más específico.");
        }

        var modelo = new Modelo(request.MarcaId, request.Nombre);
        db.Modelos.Add(modelo);
        await db.SaveChangesAsync(cancellationToken);

        return modelo.Id;
    }
}
