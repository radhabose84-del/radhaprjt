using Microsoft.Extensions.DependencyInjection;

namespace FinanceManagement.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddFinanceApplicationServices(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(DependencyInjection).Assembly);
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
            return services;
        }
    }
}
