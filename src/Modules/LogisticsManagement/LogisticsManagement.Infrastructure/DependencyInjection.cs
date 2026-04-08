using System.Data;
using LogisticsManagement.Application.Common.Interfaces;
using LogisticsManagement.Application.Common.Interfaces.IFreightMaster;
using LogisticsManagement.Application.Common.Interfaces.IMiscMaster;
using LogisticsManagement.Application.Common.Interfaces.IMiscTypeMaster;
using LogisticsManagement.Infrastructure.Repositories.FreightMaster;
using LogisticsManagement.Infrastructure.Repositories.MiscMaster;
using LogisticsManagement.Infrastructure.Repositories.MiscTypeMaster;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using Serilog;
using LogisticsManagement.Infrastructure.Data;
using LogisticsManagement.Infrastructure.Services;
using Contracts.Interfaces.Lookups.Logistics;
using LogisticsManagement.Infrastructure.Repositories.Lookups.Logistics;

namespace LogisticsManagement.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddLogisticsInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration,
            IHostEnvironment env)
        {
            var connectionString = (configuration.GetConnectionString("DefaultConnection") ?? string.Empty)
                                                .Replace("{SERVER}", Environment.GetEnvironmentVariable("DATABASE_SERVER") ?? "")
                                                .Replace("{USER_ID}", Environment.GetEnvironmentVariable("DATABASE_USERID") ?? "")
                                                .Replace("{ENC_PASSWORD}", Environment.GetEnvironmentVariable("DATABASE_PASSWORD") ?? "");

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found or is empty.");

            // Register DbContext
            services.AddDbContext<ApplicationDbContext>((sp, options) =>
            {
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                });
            });

            services.AddTransient<IDbConnection>(sp => new SqlConnection(connectionString));

            // MongoDB
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

            services.AddLogging(b => b.AddSerilog());
            services.AddHttpContextAccessor();
            services.AddTransient<AuthTokenHandler>();

            // Miscellaneous services
            services.AddTransient<IFileUploadService, FileUploadRepository>();
            services.AddSingleton<ITimeZoneService, TimeZoneService>();
            services.AddTransient<IJwtTokenHelper, JwtTokenHelper>();
            services.AddScoped<ILogQueryService, LogQueryService>();

            // MiscTypeMaster repositories
            services.AddScoped<IMiscTypeMasterCommandRepository, MiscTypeMasterCommandRepository>();
            services.AddScoped<IMiscTypeMasterQueryRepository, MiscTypeMasterQueryRepository>();

            // MiscMaster repositories
            services.AddScoped<IMiscMasterCommandRepository, MiscMasterCommandRepository>();
            services.AddScoped<IMiscMasterQueryRepository, MiscMasterQueryRepository>();

            // FreightMaster repositories
            services.AddScoped<IFreightMasterCommandRepository, FreightMasterCommandRepository>();
            services.AddScoped<IFreightMasterQueryRepository, FreightMasterQueryRepository>();

            // Lookup — caching handled globally by AddLookupCaching() in Program.cs
            services.AddScoped<IFreightMasterLookup, FreightMasterLookupRepository>();

            return services;
        }
    }
}
