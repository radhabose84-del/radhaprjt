using Contracts.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.Services;

namespace Shared.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registers shared infrastructure services used by all modules.
    /// Call this once in Program.cs after AddHttpContextAccessor().
    /// </summary>
    public static IServiceCollection AddSharedInfrastructureServices(this IServiceCollection services)
    {
        services.AddScoped<IIPAddressService, IPAddressService>();
        return services;
    }
}
