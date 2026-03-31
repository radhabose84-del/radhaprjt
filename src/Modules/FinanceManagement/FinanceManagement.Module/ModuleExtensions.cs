using Contracts.Commands.Finance;
using Contracts.Common;
using Contracts.Dtos.Finance;
using FinanceManagement.Application.Common.Mappings;
using FinanceManagement.Application.EInvoiceHeader.Commands.CreateEInvoiceFromSales;
using FinanceManagement.Infrastructure;
using FinanceManagement.Presentation.Validation.Common;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FinanceManagement.Module
{
    public static class ModuleExtensions
    {
        public static IServiceCollection AddFinanceManagementModule(
            this IServiceCollection services,
            IConfiguration configuration,
            IHostEnvironment env)
        {
            // ✅ 0) Infrastructure FIRST (DbContext + repos + services)
            services.AddFinanceInfrastructureServices(configuration, env);

            // ✅ 1) Use compile-time assemblies (NO Assembly.Load)
            var applicationAssembly = typeof(MappingProfile).Assembly;
            var presentationAssembly = typeof(MaxLengthProvider).Assembly;

            // ✅ 2) MediatR handlers from Application (register ALL handlers)
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));

            // ✅ 2b) Explicit registration for cross-module command handler
            // MediatR 12.x TryAddTransient may skip handlers when AddMediatR is called multiple times
            services.AddTransient<IRequestHandler<CreateEInvoiceFromSalesCommand, ApiResponseDTO<EInvoiceCreationResultDto>>,
                CreateEInvoiceFromSalesCommandHandler>();

            // ✅ 3) AutoMapper profiles from Application (register ALL profiles)
            services.AddAutoMapper(applicationAssembly);

            // ✅ 4) Validators from Presentation (register ALL validators)
            services.AddValidatorsFromAssembly(presentationAssembly);

            // ✅ 5) Module-specific validation infrastructure (used by validators)
            services.AddScoped<MaxLengthProvider>();
            services.AddScoped<IMaxLengthProvider>(sp => sp.GetRequiredService<MaxLengthProvider>());

            return services;
        }
    }
}
