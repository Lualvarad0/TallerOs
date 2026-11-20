using FluentValidation;

namespace TallerOS.Application.Comun.Cqrs;

/// <summary>Corre todos los IValidator&lt;TRequest&gt; registrados antes de llegar al handler.
/// Si uno falla, el handler ni se instancia.</summary>
public sealed class ValidationBehavior<TRequest, TRespuesta>(IEnumerable<IValidator<TRequest>> validadores)
    : IPipelineBehavior<TRequest, TRespuesta>
    where TRequest : IRequest<TRespuesta>
{
    public async Task<TRespuesta> Handle(TRequest request, SiguientePaso<TRespuesta> siguiente, CancellationToken cancellationToken)
    {
        if (validadores.Any())
        {
            var contexto = new ValidationContext<TRequest>(request);
            var resultados = await Task.WhenAll(validadores.Select(v => v.ValidateAsync(contexto, cancellationToken)));
            var errores = resultados.SelectMany(r => r.Errors).Where(e => e != null).ToList();
            if (errores.Count > 0)
            {
                throw new ValidationException(errores);
            }
        }

        return await siguiente();
    }
}
