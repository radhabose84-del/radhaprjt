using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using MaintenanceManagement.Infrastructure;
using MaintenanceManagement.API.Validation.MachineGroup;
using MaintenanceManagement.Application.Common.Mappings; // MachineGroupProfile lives in Application

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
            services.AddMaintenanceInfrastructure(configuration);

            // ✅ 2) Use compile-time assemblies (NO Assembly.Load)
            var applicationAssembly = typeof(MachineGroupProfile).Assembly;                 // MaintenanceManagement.Application
            var apiAssembly = typeof(CreateMachineGroupCommandValidator).Assembly;          // MaintenanceManagement.API

            // ✅ 3) MediatR handlers from Application
            // services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));

            // ✅ 4) AutoMapper from Application
            services.AddAutoMapper(applicationAssembly);

            // ✅ 5) Validators from API
            services.AddValidatorsFromAssembly(apiAssembly);

            // ✅ 6) Module validation helpers (your existing pattern)
            services.AddScoped<MaintenanceManagement.API.Validation.Common.MaxLengthProvider>();
            services.AddScoped<MaintenanceManagement.API.Validation.Common.IMaxLengthProvider>(
                sp => sp.GetRequiredService<MaintenanceManagement.API.Validation.Common.MaxLengthProvider>());

            return services;
        }
    }
}
