using Microsoft.EntityFrameworkCore;
using TallerOS.Application.Catalogos;
using TallerOS.Application.Comun;
using TallerOS.Application.Comun.Cqrs;
using TallerOS.Application.Comun.Excepciones;
using TallerOS.Domain.Catalogos;

namespace TallerOS.Application.Catalogos.Marcas.Commands.CrearMarca;

public sealed class CrearMarcaCommandHandler(IAppDbContext db, ICatalogoSimilitudService similitud)
    : IRequestHandler<CrearMarcaCommand, Guid>
{
    public async Task<Guid> Handle(CrearMarcaCommand request, CancellationToken cancellationToken)
    {
        var existeTipo = await db.TiposVehiculo.AnyAsync(t => t.Id == request.TipoVehiculoId, cancellationToken);
        if (!existeTipo)
        {
            throw new EntidadNoEncontradaException("TipoVehiculo", request.TipoVehiculoId);
        }

        var parecidas = await similitud.BuscarMarcasParecidasAsync(request.TipoVehiculoId, request.Nombre, cancellationToken);
        if (parecidas.Count > 0)
        {
            throw new ConflictoDeNegocioException(
                $"Ya existe una marca parecida: {string.Join(", ", parecidas)}. Si es una marca distinta, usa un nombre más específico.");
        }

        var marca = new Marca(request.TipoVehiculoId, request.Nombre);
        db.Marcas.Add(marca);
        await db.SaveChangesAsync(cancellationToken);

        return marca.Id;
    }
}
