using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace TallerOS.Application.Comun.Cqrs;

/// <summary>Resuelve el handler y los behaviors por reflexión (una vez por tipo de request, con
/// caché) y los encadena. Alcanza para el volumen de requests de TallerOS; si algún día esto pesa
/// en el arranque, se puede generar en compilación, pero no hace falta todavía.</summary>
public sealed class Sender(IServiceProvider servicios) : ISender
{
    private static readonly ConcurrentDictionary<Type, Func<IServiceProvider, object, CancellationToken, Task<object?>>> Invocadores = new();

    public Task<TRespuesta> Send<TRespuesta>(IRequest<TRespuesta> request, CancellationToken cancellationToken = default)
    {
        var invocador = Invocadores.GetOrAdd(request.GetType(), CrearInvocador<TRespuesta>);
        return InvocarYConvertir<TRespuesta>(invocador, request, cancellationToken);
    }

    private async Task<TRespuesta> InvocarYConvertir<TRespuesta>(
        Func<IServiceProvider, object, CancellationToken, Task<object?>> invocador, object request, CancellationToken cancellationToken)
    {
        var resultado = await invocador(servicios, request, cancellationToken);
        return (TRespuesta)resultado!;
    }

    private static Func<IServiceProvider, object, CancellationToken, Task<object?>> CrearInvocador<TRespuesta>(Type tipoRequest)
    {
        var tipoHandler = typeof(IRequestHandler<,>).MakeGenericType(tipoRequest, typeof(TRespuesta));
        var tipoBehavior = typeof(IPipelineBehavior<,>).MakeGenericType(tipoRequest, typeof(TRespuesta));
        var metodoHandle = tipoHandler.GetMethod(nameof(IRequestHandler<IRequest<object>, object>.Handle))!;
        var metodoBehavior = tipoBehavior.GetMethod(nameof(IPipelineBehavior<IRequest<object>, object>.Handle))!;

        return async (servicios, request, cancellationToken) =>
        {
            var handler = servicios.GetRequiredService(tipoHandler);
            SiguientePaso<TRespuesta> llamarHandler = () => (Task<TRespuesta>)metodoHandle.Invoke(handler, [request, cancellationToken])!;

            var behaviors = servicios.GetServices(tipoBehavior).Cast<object>().Reverse();
            var siguiente = llamarHandler;
            foreach (var behavior in behaviors)
            {
                var pasoActual = siguiente;
                siguiente = () => (Task<TRespuesta>)metodoBehavior.Invoke(behavior, [request, pasoActual, cancellationToken])!;
            }

            return await siguiente();
        };
    }
}
