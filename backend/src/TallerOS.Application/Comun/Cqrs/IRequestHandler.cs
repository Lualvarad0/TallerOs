namespace TallerOS.Application.Comun.Cqrs;

public interface IRequestHandler<TRequest, TRespuesta> where TRequest : IRequest<TRespuesta>
{
    Task<TRespuesta> Handle(TRequest request, CancellationToken cancellationToken);
}
