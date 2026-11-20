namespace TallerOS.Application.Comun.Cqrs;

/// <summary>Lo que inyectan los endpoints de la Api: no conocen el handler concreto, solo el
/// Command/Query. Equivalente a IMediator.Send() de MediatR, pero sin su licencia.</summary>
public interface ISender
{
    Task<TRespuesta> Send<TRespuesta>(IRequest<TRespuesta> request, CancellationToken cancellationToken = default);
}
