using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TheraHub.Application.Abstractions.Mediator;

namespace TheraHub.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        RegisterHandlers(services, typeof(ICommandHandler<,>));
        RegisterHandlers(services, typeof(IQueryHandler<,>));

        return services;
    }

    private static void RegisterHandlers(IServiceCollection services, Type handlerInterface)
    {
        var handlerTypes = typeof(DependencyInjection).Assembly
            .GetTypes()
            .Where(type =>
                !type.IsAbstract &&
                !type.IsInterface &&
                type.GetInterfaces().Any(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == handlerInterface));

        foreach (var handlerType in handlerTypes)
        {
            var serviceType = handlerType
                .GetInterfaces()
                .First(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == handlerInterface);

            services.AddScoped(serviceType, handlerType);
        }
    }
}
