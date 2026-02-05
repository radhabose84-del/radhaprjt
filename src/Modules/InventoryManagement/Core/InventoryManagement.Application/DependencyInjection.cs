// using System.Reflection;
// using InventoryManagement.Application.Common.Behaviors;
// using FluentValidation;
// using MediatR;
// using Microsoft.Extensions.DependencyInjection;


// namespace InventoryManagement.Application
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
//              services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

//             // ✅ Register ValidationBehavior for all MediatR requests
//             services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

//             return services;
//         }
//     }
// }