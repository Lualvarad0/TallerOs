using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TallerOS.Application.Comun.Cqrs;

namespace TallerOS.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddScoped<ISender, Sender>();
        services.AddValidatorsFromAssembly(assembly);

        // Handlers: una interfaz cerrada (IRequestHandler<ComandoX, RespuestaX>) por clase.
        var tiposHandler = assembly.GetTypes()
            .SelectMany(t => t.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
                .Select(i => (Interfaz: i, Implementacion: t)));

        foreach (var (interfaz, implementacion) in tiposHandler)
        {
            services.AddScoped(interfaz, implementacion);
        }

        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
