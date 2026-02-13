using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using BudgetManagement.Presentation.Validation.BudgetAllocation;
using BudgetManagement.Presentation.Validation.Common;
using BudgetManagement.Application.Common.Mappings;
using BudgetManagement.Infrastructure;
namespace BudgetManagement.Module;

public static class ModuleExtensions
{
        public static IServiceCollection AddBudgetManagementModule(
            this IServiceCollection services,
            IConfiguration configuration,
            IHostEnvironment env)
        {
            // 1) Register infrastructure services first so repos/EF are ready
            services.AddBudgetInfrastructure(configuration, env);

        // 2) Use compile-time assemblies (no Assembly.Load)
        var applicationAssembly = typeof(BudgetGroupMappingProfile).Assembly;
        var apiAssembly = typeof(CreateBudgetAllocationCommandValidator).Assembly;

        // 3) Register MediatR handlers
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));

        // 4) Register AutoMapper profiles
        services.AddAutoMapper(applicationAssembly);

        // 5) Register FluentValidation validators
        services.AddValidatorsFromAssembly(apiAssembly);

        // 6) Validation helpers used by validators
        services.AddScoped<MaxLengthProvider>();

        return services;
    }
}
