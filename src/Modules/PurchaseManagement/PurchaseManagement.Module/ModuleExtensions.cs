using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FluentValidation;
using PurchaseManagement.Infrastructure;
using PurchaseManagement.API.Validation.Common;
using PurchaseManagement.Application.Mappings.PurchaseOrder;
using PurchaseManagement.API.Validation.PurchaseOrder.Local;
using Shared.Validation.Common;

namespace PurchaseManagement.Module
{
    public static class ModuleExtensions
    {
        public static IServiceCollection AddPurchaseManagementModule(
            this IServiceCollection services,
            IConfiguration configuration,
            IHostEnvironment env)
        {
            // ✅ 0) Infrastructure FIRST (DbContext + repos + services)
            services.AddPurchaseInfrastructureServices(configuration, env);

            // ✅ 1) Use compile-time assemblies (NO Assembly.Load)
            var applicationAssembly = typeof(PurchaseOrderLocalProfile).Assembly;
            var apiAssembly = typeof(CreatePurchaseOrderValidator).Assembly;

            // ✅ 2) MediatR handlers from Application (register ALL handlers)
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));

            // ✅ 3) AutoMapper profiles from Application (register ALL profiles)
            services.AddAutoMapper(applicationAssembly);

            // ✅ 4) Validators from API (register ALL validators)
            services.AddValidatorsFromAssembly(apiAssembly);

            // ✅ 5) Module-specific validation infrastructure (used by validators)
            services.AddScoped<MaxLengthProvider>();
            services.AddScoped<IMaxLengthProvider>(sp => sp.GetRequiredService<MaxLengthProvider>());
            // ✅ 6) Provide List<ValidationRule> for validators that inject it
            services.AddScoped<List<ValidationRule>>(sp =>
                sp.GetRequiredService<IValidationRuleProvider>().GetRules().ToList());


            return services;
        }
    }
}