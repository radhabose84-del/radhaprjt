
// using Contracts.Interfaces.External.IParty;
// using Contracts.Interfaces.External.IUser;
// using Contracts.Interfaces.External.IWarehouse;
// using Contracts.Interfaces.External.IWorkflow;
// using GrpcServices.BackgroundService;
// using GrpcServices.BackgroundService.Line;
// using GrpcServices.Party;
// using GrpcServices.UserManagement;
// using GrpcServices.UserManagement.DivisionUnit;
// using GrpcServices.Warehouse.Bin;
// using GrpcServices.Warehouse.Rack;
// using GrpcServices.Warehouse.Warehouse;
// using InventoryManagement.Infrastructure.GrpcClients;
// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.DependencyInjection;
// using Shared.Infrastructure.HttpClientPolly;

// namespace InventoryManagement.Infrastructure
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
//             var backGroundServiceUrl = configuration["GrpcSettings:BackGroundUrl"];
//             var partyManagementUrl = configuration["GrpcSettings:PartyManagementUrl"];
//             var warehouseManagementUrl = configuration["GrpcSettings:WarehouseManagementUrl"];

            
//             // ✅ Register Session gRPC Client
//             services.AddGrpcClient<SessionService.SessionServiceClient>(options =>
//             {
//                 options.Address = new Uri(userManagementUrl);
//             })
//             .ConfigurePrimaryHttpMessageHandler(() => GrpcHttpHandler)
//             .AddPolicyHandler(HttpClientPolicyExtensions.GetRetryPolicy())
//             .AddPolicyHandler(HttpClientPolicyExtensions.GetCircuitBreakerPolicy());

//             services.AddScoped<IUserSessionGrpcClient, GrpcUserSessionClient>();
//             //unit grpc
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

            
//             //country grpc
//               services.AddGrpcClient<CountryService.CountryServiceClient>(options =>
//             {
//                 options.Address = new Uri(userManagementUrl);
//             })
//             .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
//             {
//                 ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
//             })
//             .AddPolicyHandler(HttpClientPolicyExtensions.GetRetryPolicy())
//             .AddPolicyHandler(HttpClientPolicyExtensions.GetCircuitBreakerPolicy());

//             services.AddScoped<ICountryGrpcClient,CountryGrpcClient>();
//              //Rack grpc
//             services.AddGrpcClient<RackService.RackServiceClient>(options =>
//             {
//                 options.Address = new Uri(warehouseManagementUrl);
//             })
//             .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
//             {
//                 ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
//             })
//             .AddPolicyHandler(HttpClientPolicyExtensions.GetRetryPolicy())
//             .AddPolicyHandler(HttpClientPolicyExtensions.GetCircuitBreakerPolicy());

//             services.AddScoped<IRackGrpcClient, RackGrpcClient>();
//              //Bin grpc
//             services.AddGrpcClient<BinService.BinServiceClient>(options =>
//             {
//                 options.Address = new Uri(warehouseManagementUrl);
//             })
//             .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
//             {
//                 ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
//             })
//             .AddPolicyHandler(HttpClientPolicyExtensions.GetRetryPolicy())
//             .AddPolicyHandler(HttpClientPolicyExtensions.GetCircuitBreakerPolicy());
//             services.AddScoped<IBinGrpcClient, BinGrpcClient>();
//              //Warehouse grpc
//             services.AddGrpcClient<WarehouseService.WarehouseServiceClient>(options =>
//             {
//                 options.Address = new Uri(warehouseManagementUrl);
//             })
//             .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
//             {
//                 ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
//             })
//             .AddPolicyHandler(HttpClientPolicyExtensions.GetRetryPolicy())
//             .AddPolicyHandler(HttpClientPolicyExtensions.GetCircuitBreakerPolicy());
//             services.AddScoped<IWarehouseGrpcClient, WarehouseGrpcClient>();

//             //Department

//             services.AddGrpcClient<DepartmentAllService.DepartmentAllServiceClient>(options =>
//             {
//                 options.Address = new Uri(userManagementUrl);
//             })
//             .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
//             {
//                 ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
//             })
//             .AddPolicyHandler(HttpClientPolicyExtensions.GetRetryPolicy())
//             .AddPolicyHandler(HttpClientPolicyExtensions.GetCircuitBreakerPolicy());

//             services.AddScoped<IDepartmentAllGrpcClient,  DepartmentGrpcClient>();

//              services.AddGrpcClient<ApprovalRequestStatusAllService.ApprovalRequestStatusAllServiceClient>(options =>
//             {
//                 options.Address = new Uri(backGroundServiceUrl);
//             })
//             .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
//             {
//                 ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
//             })
//             .AddPolicyHandler(HttpClientPolicyExtensions.GetRetryPolicy())
//             .AddPolicyHandler(HttpClientPolicyExtensions.GetCircuitBreakerPolicy());

//              services.AddGrpcClient<ApprovalRequestLineStatusService.ApprovalRequestLineStatusServiceClient>(options =>
//             {
//                 options.Address = new Uri(backGroundServiceUrl);
//             })
//             .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
//             {
//                 ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
//             })
//             .AddPolicyHandler(HttpClientPolicyExtensions.GetRetryPolicy())
//             .AddPolicyHandler(HttpClientPolicyExtensions.GetCircuitBreakerPolicy());

//             services.AddScoped<IWorkflowGrpcClient, WorkflowGrpcClient>();

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

//             services.AddScoped<IUsersAllGrpcClient, UsersGrpcClient>();
//              services.AddGrpcClient<ApproverService.ApproverServiceClient>(options =>
//             {
//                 options.Address = new Uri(backGroundServiceUrl);
//             })
//             .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
//             {
//                 ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
//             })
//             .AddPolicyHandler(HttpClientPolicyExtensions.GetRetryPolicy())
//             .AddPolicyHandler(HttpClientPolicyExtensions.GetCircuitBreakerPolicy());

//             services.AddGrpcClient<DivisionUnitService.DivisionUnitServiceClient>(options =>
//             {
//                 options.Address = new Uri(userManagementUrl);
//             })
//             .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
//             {
//                 ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
//             })
//             .AddPolicyHandler(HttpClientPolicyExtensions.GetRetryPolicy())
//             .AddPolicyHandler(HttpClientPolicyExtensions.GetCircuitBreakerPolicy());

//             services.AddScoped<IDivisionUnitGrpcClient, DivisionUnitGrpcClient>();


//             return services;
//         }
//     }
// }
