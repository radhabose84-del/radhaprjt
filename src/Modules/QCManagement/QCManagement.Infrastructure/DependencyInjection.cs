using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using QCManagement.Application.Common.Interfaces;
using QCManagement.Application.Common.Interfaces.AuditLog;
using QCManagement.Application.Common.Interfaces.IMiscMaster;
using QCManagement.Application.Common.Interfaces.IMiscTypeMaster;
using QCManagement.Application.Common.Interfaces.IQualityParameter;
using QCManagement.Application.Common.Interfaces.IQcInspection;
using QCManagement.Application.Common.Interfaces.IQualitySpecification;
using QCManagement.Application.Common.Interfaces.IQualityTemplate;
using QCManagement.Application.Common.Services;
using QCManagement.Infrastructure.Data;
using QCManagement.Infrastructure.Persistence;
using QCManagement.Infrastructure.Repositories.AuditLog;
using QCManagement.Infrastructure.Repositories.MiscMaster;
using QCManagement.Infrastructure.Repositories.MiscTypeMaster;
using QCManagement.Infrastructure.Repositories.QualityParameter;
using QCManagement.Infrastructure.Repositories.QcInspection;
using QCManagement.Infrastructure.Repositories.QualitySpecification;
using QCManagement.Infrastructure.Repositories.QualityTemplate;
using QCManagement.Infrastructure.Repositories.Lookups;
using QCManagement.Infrastructure.Logging;
using QCManagement.Infrastructure.Services;
using Contracts.Interfaces.Lookups.QC;
using Serilog;

namespace QCManagement.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddQCInfrastructureServices(
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

            // Field-level audit interceptor (Previous → New value capture)
            services.AddScoped<ActivityLogSaveChangesInterceptor>();

            // Register ApplicationDbContext with SQL Server
            services.AddDbContext<ApplicationDbContext>((sp, options) =>
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                })
                .AddInterceptors(sp.GetRequiredService<ActivityLogSaveChangesInterceptor>()));

            // Register IDbConnection for Dapper
            services.AddTransient<IDbConnection>(sp => new SqlConnection(connectionString));

            // MongoDB Context
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

            // ── Core Services ────────────────────────────────────────────────
            services.AddScoped<IAuditLogRepository, AuditLogRepository>();
            services.AddTransient<IFileUploadService, FileUploadRepository>();
            services.AddSingleton<ITimeZoneService, TimeZoneService>();
            services.AddTransient<IJwtTokenHelper, JwtTokenHelper>();
            services.AddScoped<ILogQueryService, LogQueryService>();

            // ── MiscTypeMaster ───────────────────────────────────────────────
            services.AddScoped<IMiscTypeMasterCommandRepository, MiscTypeMasterCommandRepository>();
            services.AddScoped<IMiscTypeMasterQueryRepository, MiscTypeMasterQueryRepository>();

            // ── MiscMaster ───────────────────────────────────────────────────
            services.AddScoped<IMiscMasterCommandRepository, MiscMasterCommandRepository>();
            services.AddScoped<IMiscMasterQueryRepository, MiscMasterQueryRepository>();

            // ── QualityParameter ─────────────────────────────────────────────
            services.AddScoped<IQualityParameterCommandRepository, QualityParameterCommandRepository>();
            services.AddScoped<IQualityParameterQueryRepository, QualityParameterQueryRepository>();

            // ── QualityTemplate ──────────────────────────────────────────────
            services.AddScoped<IQualityTemplateCommandRepository, QualityTemplateCommandRepository>();
            services.AddScoped<IQualityTemplateQueryRepository, QualityTemplateQueryRepository>();

            // ── QualitySpecification ─────────────────────────────────────────
            services.AddScoped<IQualitySpecificationCommandRepository, QualitySpecificationCommandRepository>();
            services.AddScoped<IQualitySpecificationQueryRepository, QualitySpecificationQueryRepository>();

            // Cross-module lookup — Purchase GRN flags lines that have a QC template available
            // (by ItemId or ItemCategoryId) within the EffectiveFrom/EffectiveTo window.
            // Cached automatically by AddLookupCaching() (interface name ends with "Lookup").
            services.AddScoped<IQualitySpecificationLookup, QualitySpecificationLookupRepository>();

            // Cross-module lookup — Purchase OCR Entry selects a quality template and renders
            // its parameters dynamically. Cached automatically by AddLookupCaching().
            services.AddScoped<IQualityTemplateLookup, QualityTemplateLookupRepository>();

            // Cross-module lookup — Purchase (Arrival/GRN) resolves QcStatusId → status name from
            // QC.MiscMaster without a cross-module JOIN. Cached automatically by AddLookupCaching().
            services.AddScoped<IQcMiscMasterLookup, QcMiscMasterLookupRepository>();

            // ── QcInspection (SCRUM-1667) ────────────────────────────────────
            services.AddScoped<IQcInspectionCommandRepository, QcInspectionCommandRepository>();
            services.AddScoped<IQcInspectionQueryRepository, QcInspectionQueryRepository>();
            services.AddSingleton<IInspectionEvaluator, InspectionEvaluator>();

            return services;
        }
    }
}
