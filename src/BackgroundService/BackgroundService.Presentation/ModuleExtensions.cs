#nullable disable
using FluentValidation;
using BackgroundService.Infrastructure;
using BackgroundService.Application.Mappings;
using BackgroundService.Presentation.Validation.MiscMaster;
using BackgroundService.Presentation.Validation.Common;

namespace BackgroundService.Presentation
{
    public static class ModuleExtensions
    {
        public static IServiceCollection AddBackgroundServiceModule(
            this IServiceCollection services,
            IConfiguration configuration,
            IHostEnvironment env)
        {
            // ✅ 0) Infrastructure FIRST (DbContext + repos + Hangfire + services)
            services.AddInfrastructureServices(configuration, services);

            // ✅ 1) Use compile-time assemblies (NO Assembly.Load)
            var applicationAssembly = typeof(MiscMasterProfile).Assembly;
            var presentationAssembly = typeof(CreateMiscMasterCommandValidator).Assembly;

            // ✅ 2) MediatR handlers from Application (register ALL handlers)
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));

            // ✅ 3) AutoMapper profiles from Application (register ALL profiles)
            services.AddAutoMapper(applicationAssembly);

            // ✅ 4) Validators from Presentation (register ALL validators)
            services.AddValidatorsFromAssembly(presentationAssembly);

            // ✅ 5) Module-specific validation infrastructure (used by validators)
            services.AddScoped<MaxLengthProvider>();

            return services;
        }
    }
}
