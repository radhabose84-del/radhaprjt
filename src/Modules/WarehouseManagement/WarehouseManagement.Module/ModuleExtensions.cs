using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared.Validation.Common;
using WarehouseManagement.Infrastructure;
using WarehouseManagement.Application.Common.Mappings;          
using WarehouseManagement.Presentation.Validation.WarehouseMaster;
using WarehouseManagement.Presentation.Validation.Common;

namespace WarehouseManagement.Module
{
    public static class ModuleExtensions
    {
        public static IServiceCollection AddWarehouseManagementModule(
            this IServiceCollection services,
            IConfiguration configuration,
            IHostEnvironment env)
        {
            // ✅ 1) Register Infrastructure first (repos, db, services)
            services.AddWarehouseInfrastructure(configuration, env);

            // ✅ 2) Use compile-time assemblies (NO Assembly.Load)
            // Pick one Profile class from Application
            var applicationAssembly = typeof(WarehouseMasterProfile).Assembly;

            // Pick one Validator class from API
            var apiAssembly = typeof(CreateWarehouseMasterCommandValidator).Assembly;

            // ✅ 3) MediatR handlers from Application
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));

            // ✅ 4) AutoMapper from Application
            services.AddAutoMapper(applicationAssembly);

            // ✅ 5) Validators from API
            services.AddValidatorsFromAssembly(apiAssembly);

            // ✅ 6) Validation helpers (used by validators)
            services.AddScoped<MaxLengthProvider>();
            services.AddScoped<IMaxLengthProvider>(sp => sp.GetRequiredService<MaxLengthProvider>());

            // ✅ 7) Provide List<ValidationRule> for validators that inject it
            services.AddScoped<List<ValidationRule>>(sp =>
                sp.GetRequiredService<IValidationRuleProvider>().GetRules().ToList());

            return services;
        }
    }
}
