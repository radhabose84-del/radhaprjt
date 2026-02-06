// using UserManagement.Application.UserLogin.Commands.UserLogin;
// using FluentValidation;
// using MediatR;
// using Microsoft.Extensions.DependencyInjection;
// using System.Reflection;
// using System;
// using UserManagement.Application.Common.Mappings;


// namespace UserManagement.Application
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
//                 cfg.RegisterServicesFromAssembly(typeof(UserLoginCommandHandler).Assembly);
//             });
            
            
//             return services;
//         }
//     }
// }
