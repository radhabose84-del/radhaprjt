using System;
using System.Data;
using BudgetManagement.Application.Common.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using Serilog;
using BudgetManagement.Infrastructure.Data;
using BudgetManagement.Infrastructure.Services;
using BudgetManagement.Application.Common.Interfaces.IBudgetRequest;
using BudgetManagement.Infrastructure.Repositories.BudgetRequest;
using BudgetManagement.Application.Common.Interfaces.IBudgetGroupMaster;
using BudgetManagement.Application.Common.Interfaces.IMiscMaster;
using BudgetManagement.Infrastructure.Repositories;
using BudgetManagement.Infrastructure.Repositories.BudgetGroup;
using BudgetManagement.Infrastructure.Repositories.MiscMaster;
using BudgetManagement.Application.Common.Interfaces.IMiscTypeMaster;
using BudgetManagement.Application.Common.Interfaces.IBudgetAllocation;
using BudgetManagement.Infrastructure.Repositories.BudgetAllocation;
using BudgetManagement.Infrastructure.Persistence;
using Contracts.Interfaces.Lookups.Maintenance;
using Contracts.Interfaces.Lookups.Budget;
using BudgetManagement.Infrastructure.Repositories.Lookups.Budget;
// using GrpcServices.BackgroundService;
// using GrpcServices.BackgroundService.Line;
using System.Net.Http;


namespace BudgetManagement.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddBudgetInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration,
            IHostEnvironment env)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                                                .Replace("{SERVER}", Environment.GetEnvironmentVariable("DATABASE_SERVER") ?? "")
                                                .Replace("{USER_ID}", Environment.GetEnvironmentVariable("DATABASE_USERID") ?? "")
                                                .Replace("{ENC_PASSWORD}", Environment.GetEnvironmentVariable("DATABASE_PASSWORD") ?? "");

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found or is empty.");

            // ?? Register interceptor itself
            services.AddScoped<ActivityLogSaveChangesInterceptor>();

            // ? Attach interceptor to DbContext
            services.AddDbContext<ApplicationDbContext>((sp, options) =>
            {
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                });

                // ?? Resolve interceptor from DI & register with EF
                var activityInterceptor = sp.GetRequiredService<ActivityLogSaveChangesInterceptor>();
                options.AddInterceptors(activityInterceptor);
            });

            // ----- rest of your registrations unchanged -----

            services.AddTransient<IDbConnection>(sp => new SqlConnection(connectionString));

            services.AddSingleton<IMongoClient>(sp =>
            {
                var mongoConnectionString = configuration.GetConnectionString("MongoDbConnectionString");
                if (string.IsNullOrWhiteSpace(mongoConnectionString))
                    throw new InvalidOperationException("MongoDB connection string is missing or empty.");
                return new MongoClient(mongoConnectionString);
            });

            services.AddSingleton<IMongoDbContext>(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                var databaseName = configuration["MongoDb:DatabaseName"];
                if (string.IsNullOrWhiteSpace(databaseName))
                    throw new InvalidOperationException("MongoDB database name is missing or empty.");
                return new MongoDbContext(client, databaseName);
            });

            services.AddSingleton(sp =>
            {
                var mongoDbContext = (MongoDbContext)sp.GetRequiredService<IMongoDbContext>();
                return mongoDbContext.GetDatabase();
            });
              services.AddSingleton<IMongoCollection<OutboxMessage>>(sp =>
            {
                var db = sp.GetRequiredService<IMongoDatabase>();
                return db.GetCollection<OutboxMessage>("OutboxMessages");
            });


            services.AddLogging(b => b.AddSerilog());

            services.AddHttpContextAccessor();
            services.AddTransient<AuthTokenHandler>();

            services.AddScoped<IBudgetRequestCommandRepository, BudgetRequestCommandRepository>();
            services.AddScoped<IBudgetRequestQueryRepository, BudgetRequestQueryRepository>();
            services.AddScoped<IMiscMasterCommandRepository, MiscMasterCommandRepository>();
            services.AddScoped<IMiscMasterQueryRepository, MiscMasterQueryRepository>();
            services.AddScoped<IMiscTypeMasterCommandRepository, MiscTypeMasterCommandRepository>();
            services.AddScoped<IMiscTypeMasterQueryRepository, MiscTypeMasterQueryRepository>();
            services.AddScoped<IActivityLogQueryRepository, ActivityLogQueryRepository>();                  
            services.AddScoped<IBudgetAllocationLookup, BudgetAllocationLookupRepository>();


            services.AddScoped<IBudgetAllocationQueryRepository, BudgetAllocationQueryRepository>();
            services.AddScoped<IBudgetAllocationCommandRepository, BudgetAllocationCommandRepository>();
            services.AddScoped<IBudgetGroupCommandRepository, BudgetGroupCommandRepository>();
            services.AddScoped<IBudgetGroupQueryRepository, BudgetGroupQueryRepository>();
            services.AddScoped<IIPAddressService, IPAddressService>();
            services.AddTransient<IFileUploadService, FileUploadRepository>();
            services.AddSingleton<ITimeZoneService, TimeZoneService>();
            services.AddTransient<IJwtTokenHelper, JwtTokenHelper>();
            services.AddScoped<ILogQueryService, LogQueryService>();
            services.AddScoped<IEventPublisher, EventPublisher>();

            services.AddScoped<IBudgetAllocationLookup, BudgetAllocationLookupRepository>();
            services.AddScoped<IBudgetGroupLookup, BudgetGroupLookupRepository>();

            var backgroundServiceUrl = configuration["HttpClientSettings:BackgroundService"]
                                       ?? configuration["GrpcSettings:BackGroundUrl"];

            if (string.IsNullOrWhiteSpace(backgroundServiceUrl))
            {
                throw new InvalidOperationException(
                    "Background service gRPC URL is missing. Set HttpClientSettings:BackgroundService or GrpcSettings:BackGroundUrl.");
            }

            static HttpClientHandler CreateGrpcHandler() => new()
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            // services.AddGrpcClient<ApproverService.ApproverServiceClient>(options =>
            //     options.Address = new Uri(backgroundServiceUrl))
            //     .ConfigurePrimaryHttpMessageHandler(CreateGrpcHandler);

            // services.AddGrpcClient<ApprovalRequestStatusAllService.ApprovalRequestStatusAllServiceClient>(options =>
            //     options.Address = new Uri(backgroundServiceUrl))
            //     .ConfigurePrimaryHttpMessageHandler(CreateGrpcHandler);

            // services.AddGrpcClient<ApprovalRequestLineStatusService.ApprovalRequestLineStatusServiceClient>(options =>
            //     options.Address = new Uri(backgroundServiceUrl))
            //     .ConfigurePrimaryHttpMessageHandler(CreateGrpcHandler);

            //services.AddScoped<IWorkflowLookup, WorkflowLookup>();

            return services;
       }
    }

}
