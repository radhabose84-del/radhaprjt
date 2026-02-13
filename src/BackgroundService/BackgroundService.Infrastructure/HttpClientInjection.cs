// using BackgroundService.Infrastructure.GrpcClients;
// using Contracts.Interfaces.External.IFixedAssetManagement;
// using Contracts.Interfaces.External.IMaintenance;
// using Contracts.Interfaces.External.IUser;
// using System;
// using System.Net.Http;
// using GrpcServices.UserManagement;
// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.DependencyInjection;
// using Shared.Infrastructure.HttpClientPolly;

// namespace BackgroundService.Infrastructure
// {
//     public static class HttpClientInjection
//     {
//         private static readonly HttpClientHandler GrpcHttpHandler = new HttpClientHandler
//         {
//             ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
//         };
//         public static IServiceCollection AddHttpClients(this IServiceCollection services, IConfiguration configuration)
//         {
//             var userManagementUrl = configuration["GrpcSettings:UserManagementUrl"];

          

//             // ✅ Register Department gRPC Client
//             services.AddGrpcClient<DepartmentService.DepartmentServiceClient>(options =>
//             {
//                 options.Address = new Uri(userManagementUrl);
//             })
//             .ConfigurePrimaryHttpMessageHandler(() => GrpcHttpHandler)
//             // .AddGrpcPolicies();
//             .ConfigurePrimaryHttpMessageHandler(() =>
//             {
//                 return new HttpClientHandler
//                 {
//                     // ❗ Use only in development to ignore SSL validation
//                     // In production, use a valid certificate
//                     ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
//                 };
//             })
//             .AddPolicyHandler(HttpClientPolicyExtensions.GetRetryPolicy())
//             .AddPolicyHandler(HttpClientPolicyExtensions.GetCircuitBreakerPolicy());

//             services.AddScoped<IDepartmentGrpcClient, DepartmentGrpcClient>();

//             // ✅ Register Session gRPC Client
//             services.AddGrpcClient<SessionService.SessionServiceClient>(options =>
//             {
//                 options.Address = new Uri(userManagementUrl);
//             })
//             .ConfigurePrimaryHttpMessageHandler(() => GrpcHttpHandler)
//             // .AddGrpcPolicies();
//             .ConfigurePrimaryHttpMessageHandler(() =>
//             {
//                 return new HttpClientHandler
//                 {
//                     ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
//                 };
//             })
//             .AddPolicyHandler(HttpClientPolicyExtensions.GetRetryPolicy())
//             .AddPolicyHandler(HttpClientPolicyExtensions.GetCircuitBreakerPolicy());

//             services.AddScoped<IUserSessionGrpcClient, GrpcUserSessionClient>();

//             // ✅ Register Session gRPC Client
//             services.AddGrpcClient<SessionService.SessionServiceClient>(options =>
//             {
//                 options.Address = new Uri(userManagementUrl);
//             })
//             .ConfigurePrimaryHttpMessageHandler(() => GrpcHttpHandler)
//             // .AddGrpcPolicies();
//             .ConfigurePrimaryHttpMessageHandler(() =>
//             {
//                 return new HttpClientHandler
//                 {
//                     ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
//                 };
//             })
//             .AddPolicyHandler(HttpClientPolicyExtensions.GetRetryPolicy())
//             .AddPolicyHandler(HttpClientPolicyExtensions.GetCircuitBreakerPolicy());

//             services.AddScoped<IUserSessionGrpcClient, GrpcUserSessionClient>();

//             services.AddGrpcClient<GetAllUsersJobService.GetAllUsersJobServiceClient>(options =>
//             {
//                 options.Address = new Uri(userManagementUrl);
//             })
//             .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
//             {
//                 ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
//             })
//             .AddPolicyHandler(HttpClientPolicyExtensions.GetRetryPolicy())
//             .AddPolicyHandler(HttpClientPolicyExtensions.GetCircuitBreakerPolicy());
//             services.AddScoped<IUsersAllGrpcClient, GrpcGetAllUserClient>();

//             services.AddGrpcClient<UnitService.UnitServiceClient>(options =>
//             {
//                 options.Address = new Uri(userManagementUrl);
//             })
//             .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
//             {
//                 ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
//             })
//             .AddPolicyHandler(HttpClientPolicyExtensions.GetRetryPolicy())
//             .AddPolicyHandler(HttpClientPolicyExtensions.GetCircuitBreakerPolicy());
//             services.AddScoped<IUnitGrpcClient, UnitGrpcClient>();

//               services.AddGrpcClient<MenuService.MenuServiceClient>(options =>
//             {
//                 options.Address = new Uri(userManagementUrl);
//             })
//             .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
//             {
//                 ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
//             })
//             .AddPolicyHandler(HttpClientPolicyExtensions.GetRetryPolicy())
//             .AddPolicyHandler(HttpClientPolicyExtensions.GetCircuitBreakerPolicy());
//             services.AddScoped<IMenuGrpcClient, MenuGrpcClient>();


        
//             return services;
//         }
//     }
// }
