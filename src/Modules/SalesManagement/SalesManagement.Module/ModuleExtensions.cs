using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SalesManagement.Infrastructure;
using SalesManagement.Application.Common.Mappings;
using SalesManagement.API.Validation.Common;
using SalesManagement.Application;

namespace SalesManagement.Module
{
    public static class ModuleExtensions
    {
        public static IServiceCollection AddSalesManagementModule(
            this IServiceCollection services,
            IConfiguration configuration,
            IHostEnvironment env)
        {
            // ✅ 1) Register Infrastructure first (repos, db, services)
            services.AddSalesInfrastructure(configuration);

            services.AddApplicationServices();


            // ✅ 2) Use compile-time assemblies (NO Assembly.Load)
            // var applicationAssembly = typeof(MachineGroupProfile).Assembly;                 
            // var apiAssembly = typeof(CreateMachineGroupCommandValidator).Assembly;          

            // // ✅ 3) MediatR handlers from Application
            // services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));

            // // ✅ 4) AutoMapper from Application
            // services.AddAutoMapper(applicationAssembly);

            // // ✅ 5) Validators from API
            // services.AddValidatorsFromAssembly(apiAssembly);

            // ✅ 5) Validation helpers (used by validators)
            services.AddScoped<MaxLengthProvider>();
            services.AddScoped<IMaxLengthProvider>(sp => sp.GetRequiredService<MaxLengthProvider>());


            return services;
        }
    }
}
