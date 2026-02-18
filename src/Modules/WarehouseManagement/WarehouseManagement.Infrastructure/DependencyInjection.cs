#nullable disable
using System.Data;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Common.Interfaces.AuditLog;
using WarehouseManagement.Application.Common.Interfaces.IBinMaster;
using WarehouseManagement.Application.Common.Interfaces.IRackMaster;
using WarehouseManagement.Application.Common.Interfaces.IWarehouseMaster;
using WarehouseManagement.Application.Common.Mappings;
using Infrastructure.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using PartyManagement.Infrastructure.Repositories;
using Serilog;
using WarehouseManagement.Infrastructure.Data;
using WarehouseManagement.Infrastructure.Repositories.BinMaster;
using WarehouseManagement.Infrastructure.Repositories.Lookups;
using WarehouseManagement.Infrastructure.Repositories.RackMaster;
using WarehouseManagement.Infrastructure.Repositories.WarehouseMaster;
using WarehouseManagement.Infrastructure.Services;
using WarehouseManagement.Application.WarehouseMaster.Services;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Warehouse;

namespace WarehouseManagement.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddWarehouseInfrastructure(this IServiceCollection services, IConfiguration configuration, IHostEnvironment env)
        {

            var connectionString = configuration.GetConnectionString("DefaultConnection")
                                                .Replace("{SERVER}", Environment.GetEnvironmentVariable("DATABASE_SERVER") ?? "")
                                                .Replace("{USER_ID}", Environment.GetEnvironmentVariable("DATABASE_USERID") ?? "")
                                                .Replace("{ENC_PASSWORD}", Environment.GetEnvironmentVariable("DATABASE_PASSWORD") ?? "");


            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found or is empty.");
            }

            // Register ApplicationDbContext with SQL Server

            services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5, // Number of retry attempts
                    maxRetryDelay: TimeSpan.FromSeconds(30), // Delay between retries
                    errorNumbersToAdd: null); // Add specific SQL error numbers to retry on (optional)
            }));

            // Register IDbConnection for Dapper
            services.AddTransient<IDbConnection>(sp => new SqlConnection(connectionString));


            // MongoDB Context
            services.AddSingleton<IMongoClient>(sp =>
            {
                var mongoConnectionString = configuration.GetConnectionString("MongoDbConnectionString");
                if (string.IsNullOrWhiteSpace(mongoConnectionString))
                {
                    throw new InvalidOperationException("MongoDB connection string is missing or empty.");
                }
                return new MongoClient(mongoConnectionString);
            });

            services.AddSingleton<IMongoDbContext>(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                var databaseName = configuration["MongoDb:DatabaseName"];
                if (string.IsNullOrWhiteSpace(databaseName))
                {
                    throw new InvalidOperationException("MongoDB database name is missing or empty.");
                }
                return new MongoDbContext(client, databaseName);
            });

            // Optional: Register IMongoDatabase if needed directly
            services.AddSingleton(sp =>
            {
                var mongoDbContext = (MongoDbContext)sp.GetRequiredService<IMongoDbContext>();
                return mongoDbContext.GetDatabase();
            });


            // Register ILogger<T>
            services.AddLogging(builder =>
            {
                builder.AddSerilog();
            });

            // Register IDateTime
            services.AddHttpContextAccessor();
            services.AddTransient<AuthTokenHandler>();

            // Register repositories
            services.AddScoped<IAuditLogRepository, AuditLogRepository>();
            services.AddScoped<IWarehouseMasterQueryRepository , WarehouseMasterQueryRepository >();
            services.AddScoped<IWarehouseMasterCommandRepository, WarehouseMasterCommandRepository>();
            services.AddScoped<IWarehouseCodeGenerator, WarehouseCodeGenerator>();
            services.AddScoped<IRackMasterQueryRepository, RackMasterQueryRepository>();
            services.AddScoped<IRackMasterCommandRepository, RackMasterCommandRepository>();
            // services.AddScoped<IRackCodeGenerator, RackCodeGenerator>();
            services.AddScoped<IBinMasterQueryRepository, BinMasterQueryRepository>();
            services.AddScoped<IBinMasterCommandRepository, BinMasterCommandRepository>();
            services.AddScoped<IBinCodeGenerator, BinCodeGenerator>();

            // Lookups
            services.AddScoped<IWarehouseLookup, WarehouseLookupRepository>();
            services.AddScoped<IRackLookup, RackLookupRepository>();
            services.AddScoped<IBinLookup, BinLookupRepository>();
            services.AddScoped<IItemGroupLookup, ItemGroupLookupRepository>();
            services.AddScoped<ILocationLookup, LocationLookupRepository>();
            services.AddScoped<IMiscMasterLookup, MiscMasterLookupRepository>();
            services.AddScoped<IUnitLookup, UnitLookupRepository>();
    



            // Miscellaneous services
            services.AddScoped<IIPAddressService, IPAddressService>();
            services.AddTransient<IFileUploadService, FileUploadRepository>();
            services.AddSingleton<ITimeZoneService, TimeZoneService>();
            services.AddTransient<IJwtTokenHelper, JwtTokenHelper>();
            // AutoMapper profiles
           
             services.AddAutoMapper(
                 typeof(WarehouseMasterProfile),
                 typeof(RackMasterProfile),
                 typeof(BinMasterProfile)

            );
            return services;
        }

    }
}
