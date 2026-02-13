using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PartyManagement.Presentation.Validation.Common;
using PartyManagement.Presentation.Validation.PartyMaster;
using PartyManagement.Application.Common.Mappings;
using Shared.Validation.Common;
using PartyManagement.Infrastructure;
using Microsoft.Extensions.Hosting;

namespace PartyManagement.Module
{
    public static class ModuleExtensions
    {
        public static IServiceCollection AddPartyManagementModule(
            this IServiceCollection services,
            IConfiguration configuration,IHostEnvironment env)
        {
            // ✅ 1) Register Infrastructure first (repos, db, services)
            services.AddPartyInfrastructure(configuration, env);

            // ✅ 2) Use compile-time assemblies (NO Assembly.Load)
            var applicationAssembly = typeof(PartyMasterProfile).Assembly;              
            var apiAssembly = typeof(CreatePartyMasterCommandValidator).Assembly;           

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