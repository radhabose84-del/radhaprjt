using Microsoft.Extensions.DependencyInjection;

namespace ProductionManagement.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddProductionApplicationServices(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(DependencyInjection).Assembly);
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
            return services;
        }
    }
}
