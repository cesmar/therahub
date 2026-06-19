using Microsoft.Extensions.DependencyInjection;
using TheraHub.Application.Abstractions.Mediator;
using TheraHub.Infrastructure.Mediator;

namespace TheraHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<ISender, Sender>();
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
