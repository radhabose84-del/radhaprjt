using FinanceManagement.Application.Common.Behaviors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace FinanceManagement.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddFinanceApplicationServices(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(DependencyInjection).Assembly);
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

            // US-GL02-FR-008a: translate the DB freeze-trigger error into a friendly 400 + log the attempt.
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CoaFreezeViolationBehavior<,>));
            return services;
        }
    }
}
