using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SalesManagement.Infrastructure;
using SalesManagement.Application.Common.Mappings;
using SalesManagement.Presentation.Validation.Common;
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
            // 1) Register Infrastructure first (repos, db, services)
            services.AddSalesInfrastructure(configuration);

            // 2) Application services (MediatR + AutoMapper via reflection)
            services.AddApplicationServices();

         

            // 4) Validation helpers
            services.AddScoped<MaxLengthProvider>();
            services.AddScoped<IMaxLengthProvider>(sp => sp.GetRequiredService<MaxLengthProvider>());

            return services;
        }
    }
}
