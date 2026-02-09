using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Shared.Validation.Common;

using ProjectManagement.Infrastructure;

// ✅ Change these 2 marker types to your real types
using ProjectManagement.Application.Common.Mappings;
using ProjectManagement.API.Validation.ProjectMaster;

namespace ProjectManagement.Module
{
    public static class ModuleExtensions
    {
        public static IServiceCollection AddProjectManagementModule(
            this IServiceCollection services,
            IConfiguration configuration,
            IHostEnvironment env)
        {
            // ✅ 1) Infrastructure
            services.AddProjectInfrastructure(configuration, env);

            // ✅ 2) Assembly markers (NO Assembly.Load)
            var applicationAssembly = typeof(ProjectProfile).Assembly;
            var apiAssembly = typeof(CreateProjectCommandValidator).Assembly;

            // ✅ 3) MediatR
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));

            // ✅ 4) AutoMapper
            services.AddAutoMapper(applicationAssembly);

            // ✅ 5) Validators
            services.AddValidatorsFromAssembly(apiAssembly);

            // ✅ 6) MaxLength provider
            services.AddScoped<MaxLengthProvider>();
            services.AddScoped<IMaxLengthProvider>(sp => sp.GetRequiredService<MaxLengthProvider>());

            // ✅ 7) Validation rules list
            services.AddScoped<List<ValidationRule>>(sp =>
                sp.GetRequiredService<IValidationRuleProvider>().GetRules().ToList());

            return services;
        }
    }
}
