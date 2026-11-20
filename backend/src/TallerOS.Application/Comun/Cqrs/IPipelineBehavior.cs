namespace TallerOS.Application.Comun.Cqrs;

public delegate Task<TRespuesta> SiguientePaso<TRespuesta>();

/// <summary>Middleware alrededor de un handler (ej. ValidationBehavior). Se registran en orden en
/// DependencyInjection.cs; el último registrado es el más cercano al handler real.</summary>
public interface IPipelineBehavior<TRequest, TRespuesta> where TRequest : IRequest<TRespuesta>
{
    Task<TRespuesta> Handle(TRequest request, SiguientePaso<TRespuesta> siguiente, CancellationToken cancellationToken);
}
