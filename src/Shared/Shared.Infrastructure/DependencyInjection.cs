using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Common;
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
        services.AddScoped<IDataAccessFilter, DataAccessFilterService>();
        services.AddScoped<IAppDataMiscMasterLookup, AppDataMiscMasterLookupRepository>();
        return services;
    }
}
