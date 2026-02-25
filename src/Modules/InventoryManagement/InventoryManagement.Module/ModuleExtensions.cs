using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using InventoryManagement.Infrastructure;
using InventoryManagement.Presentation.Validation.Common;
using InventoryManagement.Application.Common.Mappings.Item;
using InventoryManagement.Presentation.Validation.Item.ItemGroup;
using Shared.Validation.Common;

namespace InventoryManagement.Module
{
    public static class ModuleExtensions
    {
        public static IServiceCollection AddInventoryManagementModule(
            this IServiceCollection services,
            IConfiguration configuration,
            IHostEnvironment env)
        {
            // ✅ 1) Register Infrastructure first (repos, db, services)
            services.AddInventoryInfrastructure(configuration, env);

            // ✅ 2) Use compile-time assemblies (NO Assembly.Load)
            var applicationAssembly = typeof(ItemGroupProfile).Assembly;              // InventoryManagement.Application
            var apiAssembly = typeof(CreateItemGroupCommandValidator).Assembly;            // InventoryManagement.Presentation

            // ✅ 3) MediatR handlers from Application
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));

            // ✅ 4) AutoMapper from Application
            services.AddAutoMapper(applicationAssembly);

            // ✅ 5) Validators from API
            services.AddValidatorsFromAssembly(apiAssembly);

            // ✅ 6) Validation helpers (used by validators) - SAME AS YOUR PATTERN
            services.AddScoped<MaxLengthProvider>();
            services.AddScoped<IMaxLengthProvider>(sp => sp.GetRequiredService<MaxLengthProvider>());
            
            // ✅ 7) Provide List<ValidationRule> for validators that inject it
            services.AddScoped<List<ValidationRule>>(sp =>
                sp.GetRequiredService<IValidationRuleProvider>().GetRules().ToList());

            return services;
        }
    }
}
