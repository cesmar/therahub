using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TheraHub.Application.Abstractions.Mediator;
using TheraHub.Application.Abstractions.Tenancy;
using TheraHub.Infrastructure.Mediator;
using TheraHub.Infrastructure.Persistence;
using TheraHub.Infrastructure.Services;

namespace TheraHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ISender, Sender>();
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.AddDbContext<TheraHubDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentTenant, CurrentTenant>();

        return services;
    }
}
