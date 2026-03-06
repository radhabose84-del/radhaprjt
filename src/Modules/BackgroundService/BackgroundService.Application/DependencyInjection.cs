using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace BackgroundService.Application
{
    public static class DependencyInjection
    {
           public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            // Use a specific AddAutoMapper overload
            services.AddAutoMapper(cfg => cfg.AddMaps(Assembly.GetExecutingAssembly()));
            
        // services.AddScoped<UserUnlockservice>();
        // services.AddScoped<PreventiveScheduleService>();
            // Add MediatR
            // services.AddMediatR(cfg =>
            // {
            //     cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            //     cfg.RegisterServicesFromAssembly(typeof(SendEmailCommandHandler).Assembly);
            // });
            services.AddSignalR();            
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));            
            return services;
        }
    }
}