using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using ProductionManagement.Application.Common.Interfaces;
using ProductionManagement.Application.Common.Interfaces.AuditLog;
using ProductionManagement.Application.Common.Interfaces.ICountGroup;
using ProductionManagement.Application.Common.Interfaces.IYarnType;
using ProductionManagement.Application.Common.Interfaces.ICountMaster;
using ProductionManagement.Application.Common.Interfaces.ILotMaster;
using ProductionManagement.Application.Common.Interfaces.IMiscMaster;
using ProductionManagement.Application.Common.Interfaces.IMiscTypeMaster;
using ProductionManagement.Application.Common.Interfaces.IPackType;
using ProductionManagement.Application.Common.Interfaces.IProductionPack;
using ProductionManagement.Application.Common.Interfaces.IRepacking;
using ProductionManagement.Application.Common.Interfaces.IProcessMaster;
using ProductionManagement.Application.Common.Interfaces.IQualityMaster;
using ProductionManagement.Application.Common.Interfaces.ICertificationMaster;
using ProductionManagement.Application.Common.Interfaces.IYarnTwistMaster;
using Contracts.Interfaces.Lookups.Production;
using ProductionManagement.Infrastructure.Repositories.Lookups.Production;
using ProductionManagement.Infrastructure.Repositories.ProcessMaster;
using ProductionManagement.Infrastructure.Repositories.QualityMaster;
using ProductionManagement.Infrastructure.Repositories.CertificationMaster;
using ProductionManagement.Infrastructure.Repositories.YarnTwistMaster;
using ProductionManagement.Infrastructure.Data;
using ProductionManagement.Infrastructure.Persistence;
using ProductionManagement.Infrastructure.Repositories.AuditLog;
using ProductionManagement.Infrastructure.Repositories.CountGroup;
using ProductionManagement.Infrastructure.Repositories.YarnType;
using ProductionManagement.Infrastructure.Repositories.CountMaster;
using ProductionManagement.Infrastructure.Repositories.MiscMaster;
using ProductionManagement.Infrastructure.Repositories.MiscTypeMaster;
using ProductionManagement.Infrastructure.Services;
using Serilog;

namespace ProductionManagement.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddProductionInfrastructureServices(
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

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                }));

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

            services.AddLogging(builder => builder.AddSerilog());
            services.AddHttpContextAccessor();
            services.AddTransient<AuthTokenHandler>();

            services.AddScoped<IAuditLogRepository, AuditLogRepository>();
            services.AddTransient<IFileUploadService, FileUploadRepository>();
            services.AddSingleton<ITimeZoneService, TimeZoneService>();
            services.AddTransient<IJwtTokenHelper, JwtTokenHelper>();
            services.AddScoped<ILogQueryService, LogQueryService>();

            // CountGroup repositories
            services.AddScoped<ICountGroupCommandRepository, CountGroupCommandRepository>();
            services.AddScoped<ICountGroupQueryRepository, CountGroupQueryRepository>();

            // YarnType repositories
            services.AddScoped<IYarnTypeCommandRepository, YarnTypeCommandRepository>();
            services.AddScoped<IYarnTypeQueryRepository, YarnTypeQueryRepository>();

            // CountMaster repositories
            services.AddScoped<ICountMasterCommandRepository, CountMasterCommandRepository>();
            services.AddScoped<ICountMasterQueryRepository, CountMasterQueryRepository>();

            // MiscTypeMaster repositories
            services.AddScoped<IMiscTypeMasterCommandRepository, MiscTypeMasterCommandRepository>();
            services.AddScoped<IMiscTypeMasterQueryRepository, MiscTypeMasterQueryRepository>();

            // MiscMaster repositories
            services.AddScoped<IMiscMasterCommandRepository, MiscMasterCommandRepository>();
            services.AddScoped<IMiscMasterQueryRepository, MiscMasterQueryRepository>();

            // LotMaster repositories
            services.AddScoped<ILotMasterCommandRepository, Repositories.LotMaster.LotMasterCommandRepository>();
            services.AddScoped<ILotMasterQueryRepository, Repositories.LotMaster.LotMasterQueryRepository>();

            // PackType repositories
            services.AddScoped<IPackTypeCommandRepository, Repositories.PackType.PackTypeCommandRepository>();
            services.AddScoped<IPackTypeQueryRepository, Repositories.PackType.PackTypeQueryRepository>();

            // Pack Allocation repositories
            services.AddScoped<IProductionCommandRepository, Repositories.ProductionPack.ProductionCommandRepository>();
            services.AddScoped<IProductionQueryRepository, Repositories.ProductionPack.ProductionQueryRepository>();

            // Repacking repositories
            services.AddScoped<IRepackingCommandRepository, Repositories.Repacking.RepackingCommandRepository>();
            services.AddScoped<IRepackingQueryRepository, Repositories.Repacking.RepackingQueryRepository>();

            // ProcessMaster repositories
            services.AddScoped<IProcessMasterCommandRepository, ProcessMasterCommandRepository>();
            services.AddScoped<IProcessMasterQueryRepository, ProcessMasterQueryRepository>();

            // QualityMaster repositories
            services.AddScoped<IQualityMasterCommandRepository, QualityMasterCommandRepository>();
            services.AddScoped<IQualityMasterQueryRepository, QualityMasterQueryRepository>();

            // CertificationMaster repositories
            services.AddScoped<ICertificationMasterCommandRepository, CertificationMasterCommandRepository>();
            services.AddScoped<ICertificationMasterQueryRepository, CertificationMasterQueryRepository>();

            // YarnTwistMaster repositories
            services.AddScoped<IYarnTwistMasterCommandRepository, YarnTwistMasterCommandRepository>();
            services.AddScoped<IYarnTwistMasterQueryRepository, YarnTwistMasterQueryRepository>();

            // Lookup registration — caching is handled globally by AddLookupCaching() in Program.cs
            services.AddScoped<IYarnTypeLookup, YarnTypeLookupRepository>();
            services.AddScoped<ICountGroupLookup, CountGroupLookupRepository>();
            services.AddScoped<ICountMasterLookup, CountMasterLookupRepository>();
            services.AddScoped<ILotMasterLookup, LotMasterLookupRepository>();
            services.AddScoped<IPackTypeLookup, PackTypeLookupRepository>();
            services.AddScoped<IProcessMasterLookup, ProcessMasterLookupRepository>();
            services.AddScoped<IQualityMasterLookup, QualityMasterLookupRepository>();
            services.AddScoped<ICertificationMasterLookup, CertificationMasterLookupRepository>();
            services.AddScoped<IYarnTwistMasterLookup, YarnTwistMasterLookupRepository>();

            return services;
        }
    }
}
