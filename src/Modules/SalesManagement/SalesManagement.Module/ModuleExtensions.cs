using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FluentValidation;
using SalesManagement.Infrastructure;
using SalesManagement.Presentation.Validation.Common;
using SalesManagement.Application.Common.Mappings;
using SalesManagement.Presentation.Validation.SalesOrganisation;
using Shared.Validation.Common;

namespace SalesManagement.Module
{
    public static class ModuleExtensions
    {
        public static IServiceCollection AddSalesManagementModule(
            this IServiceCollection services,
            IConfiguration configuration,
            IHostEnvironment env)
        {
            // ✅ 0) Infrastructure FIRST (DbContext + repos + services)
            services.AddSalesInfrastructureServices(configuration, env);

            // ✅ 1) Use compile-time assemblies (NO Assembly.Load)
            var applicationAssembly = typeof(SalesOrganisationProfile).Assembly;
            var apiAssembly = typeof(CreateSalesOrganisationCommandValidator).Assembly;

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
