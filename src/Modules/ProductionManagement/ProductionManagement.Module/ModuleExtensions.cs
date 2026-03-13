using FluentValidation;
using ProductionManagement.Application.Common.Mappings;
using ProductionManagement.Infrastructure;
using ProductionManagement.Presentation.Validation.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ProductionManagement.Module
{
    public static class ModuleExtensions
    {
        public static IServiceCollection AddProductionManagementModule(
            this IServiceCollection services,
            IConfiguration configuration,
            IHostEnvironment env)
        {
            services.AddProductionInfrastructureServices(configuration, env);

            var applicationAssembly = typeof(MappingProfile).Assembly;
            var presentationAssembly = typeof(MaxLengthProvider).Assembly;

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));
            services.AddAutoMapper(applicationAssembly);
            services.AddValidatorsFromAssembly(presentationAssembly);

            services.AddScoped<MaxLengthProvider>();
            services.AddScoped<IMaxLengthProvider>(sp => sp.GetRequiredService<MaxLengthProvider>());

            return services;
        }
    }
}
