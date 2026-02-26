using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Application.Common.Interfaces.AuditLog;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Persistence;
using SalesManagement.Infrastructure.Repositories.AuditLog;
using SalesManagement.Application.Common.Interfaces.ISalesOrganisation;
using SalesManagement.Infrastructure.Repositories.SalesOrganisation;
using SalesManagement.Application.Common.Interfaces.ISalesChannel;
using SalesManagement.Infrastructure.Repositories.SalesChannel;
using SalesManagement.Application.Common.Interfaces.IBusinessUnit;
using SalesManagement.Infrastructure.Repositories.BusinessUnit;
using SalesManagement.Application.Common.Interfaces.ISalesSegment;
using SalesManagement.Infrastructure.Repositories.SalesSegment;
using SalesManagement.Application.Common.Interfaces.ISalesOffice;
using SalesManagement.Infrastructure.Repositories.SalesOffice;
using SalesManagement.Application.Common.Interfaces.ISalesGroup;
using SalesManagement.Infrastructure.Repositories.SalesGroup;
using SalesManagement.Application.Common.Interfaces.ISalesItemPriceMaster;
using SalesManagement.Infrastructure.Repositories.SalesItemPriceMaster;
using SalesManagement.Application.Common.Interfaces.IMiscTypeMaster;
using SalesManagement.Infrastructure.Repositories.MiscTypeMaster;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Infrastructure.Repositories.MiscMaster;
using SalesManagement.Application.Common.Interfaces.IAgentCommissionConfig;
using SalesManagement.Infrastructure.Repositories.AgentCommissionConfig;
using SalesManagement.Infrastructure.Services;
using Serilog;
using Microsoft.Extensions.Hosting;


namespace SalesManagement.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddSalesInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration, IHostEnvironment env)
        {
            var connectionString = (configuration.GetConnectionString("DefaultConnection") ?? string.Empty)
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
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
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

            services.AddLogging(builder =>
            {
                builder.AddSerilog();
            });

            services.AddHttpContextAccessor();
            services.AddTransient<AuthTokenHandler>();

            // ═══════════════════════════════════════════════════════════════
            // REPOSITORY REGISTRATIONS - ALL REQUIRED REPOSITORIES
            // ═══════════════════════════════════════════════════════════════
            services.AddScoped<IAuditLogRepository, AuditLogRepository>();
            // Miscellaneous services
            services.AddScoped<IIPAddressService, IPAddressService>();
            services.AddTransient<IFileUploadService, FileUploadRepository>();
            services.AddSingleton<ITimeZoneService, TimeZoneService>();
            services.AddTransient<IJwtTokenHelper, JwtTokenHelper>();
            services.AddScoped<ILogQueryService, LogQueryService>();

            // ── Employee Master Repositories ────────────────────────────────

            // ── Sales Organisation Repositories ───────────────────────────────
            services.AddScoped<ISalesOrganisationCommandRepository, SalesOrganisationCommandRepository>();
            services.AddScoped<ISalesOrganisationQueryRepository, SalesOrganisationQueryRepository>();

            // ── Sales Channel Repositories ───────────────────────────────
            services.AddScoped<ISalesChannelCommandRepository, SalesChannelCommandRepository>();
            services.AddScoped<ISalesChannelQueryRepository, SalesChannelQueryRepository>();

            // ── Business Unit Repositories ───────────────────────────────
            services.AddScoped<IBusinessUnitCommandRepository, BusinessUnitCommandRepository>();
            services.AddScoped<IBusinessUnitQueryRepository, BusinessUnitQueryRepository>();

            // ── Sales Segment Repositories ───────────────────────────────
            services.AddScoped<ISalesSegmentCommandRepository, SalesSegmentCommandRepository>();
            services.AddScoped<ISalesSegmentQueryRepository, SalesSegmentQueryRepository>();

            // ── Sales Office Repositories ───────────────────────────────
            services.AddScoped<ISalesOfficeCommandRepository, SalesOfficeCommandRepository>();
            services.AddScoped<ISalesOfficeQueryRepository, SalesOfficeQueryRepository>();

            // ── Sales Group Repositories ─────────────────────────────────
            services.AddScoped<ISalesGroupCommandRepository, SalesGroupCommandRepository>();
            services.AddScoped<ISalesGroupQueryRepository, SalesGroupQueryRepository>();
  			// ── Sales Item Price Master Repositories ─────────────────────
            services.AddScoped<ISalesItemPriceMasterCommandRepository, SalesItemPriceMasterCommandRepository>();
            services.AddScoped<ISalesItemPriceMasterQueryRepository, SalesItemPriceMasterQueryRepository>();

            // ── Misc Type Master Repositories ─────────────────────────────
            services.AddScoped<IMiscTypeMasterCommandRepository, MiscTypeMasterCommandRepository>();
            services.AddScoped<IMiscTypeMasterQueryRepository, MiscTypeMasterQueryRepository>();

            // ── Misc Master Repositories ──────────────────────────────────
            services.AddScoped<IMiscMasterCommandRepository, MiscMasterCommandRepository>();
            services.AddScoped<IMiscMasterQueryRepository, MiscMasterQueryRepository>();

            // ── Agent Commission Configuration Repositories ─────────────
            services.AddScoped<IAgentCommissionConfigCommandRepository, AgentCommissionConfigCommandRepository>();
            services.AddScoped<IAgentCommissionConfigQueryRepository, AgentCommissionConfigQueryRepository>();

            return services;
        }
    }
}