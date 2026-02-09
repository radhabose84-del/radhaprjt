// using System.Reflection;
// using MediatR;
// using Microsoft.Extensions.DependencyInjection;


// namespace BudgetManagement.Application
// {
//     public static class DependencyInjection
//     {
//         public static IServiceCollection AddApplicationServices(this IServiceCollection services)
//         {
//             if (services == null) throw new ArgumentNullException(nameof(services));

//             // Use a specific AddAutoMapper overload
//             services.AddAutoMapper(cfg => cfg.AddMaps(Assembly.GetExecutingAssembly()));
         
//             services.AddSignalR();
//             // Add MediatR
//             services.AddMediatR(cfg =>
//             {
//                 cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
//             });            

            
//             return services;
//         }
//     }
// }