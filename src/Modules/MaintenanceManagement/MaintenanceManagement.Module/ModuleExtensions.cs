using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using MaintenanceManagement.Infrastructure;
using MaintenanceManagement.Presentation.Validation.MachineGroup;
using MaintenanceManagement.Application.Common.Mappings;
using MaintenanceManagement.Presentation.Validation.Common; // MachineGroupProfile lives in Application

namespace MaintenanceManagement.Module
{
    public static class ModuleExtensions
    {
        public static IServiceCollection AddMaintenanceManagementModule(
            this IServiceCollection services,
            IConfiguration configuration,
            IHostEnvironment env)
        {
            // ✅ 1) Register Infrastructure first (repos, db, services)
            services.AddMaintenanceInfrastructure(configuration, env);

            // ✅ 2) Use compile-time assemblies (NO Assembly.Load)
            var applicationAssembly = typeof(MachineGroupProfile).Assembly;                 // MaintenanceManagement.Application
            var apiAssembly = typeof(CreateMachineGroupCommandValidator).Assembly;          // MaintenanceManagement.Presentation

            // ✅ 3) MediatR handlers from Application
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));

            // ✅ 4) AutoMapper from Application
            services.AddAutoMapper(applicationAssembly);

            // ✅ 5) Validators from API
            services.AddValidatorsFromAssembly(apiAssembly);

            // ✅ 5) Validation helpers (used by validators)
            services.AddScoped<MaxLengthProvider>();
            services.AddScoped<IMaxLengthProvider>(sp => sp.GetRequiredService<MaxLengthProvider>());

            // // ✅ 6) Module validation helpers (your existing pattern)
            // services.AddScoped<MaintenanceManagement.Presentation.Validation.Common.MaxLengthProvider>();
            // services.AddScoped<MaintenanceManagement.Presentation.Validation.Common.IMaxLengthProvider>(
            //     sp => sp.GetRequiredService<MaintenanceManagement.Presentation.Validation.Common.MaxLengthProvider>());

            return services;
        }
    }
}
