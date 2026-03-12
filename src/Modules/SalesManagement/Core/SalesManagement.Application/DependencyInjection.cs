// using System.Reflection;
// using Microsoft.Extensions.DependencyInjection;


// namespace SalesManagement.Application
// {
//     public static class DependencyInjection
//     {
//         public static IServiceCollection AddApplicationServices(this IServiceCollection services)
//         {
//             if (services == null) throw new ArgumentNullException(nameof(services));

//             // Use a specific AddAutoMapper overload
//             services.AddAutoMapper(cfg => cfg.AddMaps(Assembly.GetExecutingAssembly()));


//             // Add MediatR
//             services.AddMediatR(cfg =>
//             {
//                 cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
//             });

//             return services;
//         }
//     }
// }