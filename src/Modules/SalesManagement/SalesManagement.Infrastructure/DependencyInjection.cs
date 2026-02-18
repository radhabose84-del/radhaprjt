#nullable disable
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Application.Common.Interfaces.AuditLog;
using SalesManagement.Application.Common.Interfaces.IDesignation;
using SalesManagement.Application.Common.Interfaces.ISalesOfficeMaster;
using SalesManagement.Application.Common.Interfaces.ISalesGroupMaster;
using SalesManagement.Application.Common.Interfaces.IEmployeeMaster;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Persistence;
using SalesManagement.Infrastructure.Repositories.AuditLog;
using SalesManagement.Infrastructure.Repositories.Designation;
using SalesManagement.Infrastructure.Repositories.SalesOfficeMaster;
using SalesManagement.Infrastructure.Repositories.SalesGroupMaster;
using SalesManagement.Infrastructure.Repositories.EmployeeMaster;
using SalesManagement.Infrastructure.Services;
using Serilog;


namespace SalesManagement.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddSalesInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
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
            services.AddScoped<IDesignationCommandRepository, DesignationCommandRepository>();
            services.AddScoped<IDesignationQueryRepository, DesignationQueryRepository>();
            services.AddScoped<ISalesOfficeMasterCommandRepository, SalesOfficeMasterCommandRepository>();
            services.AddScoped<ISalesOfficeMasterQueryRepository, SalesOfficeMasterQueryRepository>();
            services.AddScoped<ISalesGroupMasterCommandRepository, SalesGroupMasterCommandRepository>();
            services.AddScoped<ISalesGroupMasterQueryRepository, SalesGroupMasterQueryRepository>();
            services.AddScoped<IEmployeeMasterCommandRepository, EmployeeMasterCommandRepository>();
            services.AddScoped<IEmployeeMasterQueryRepository, EmployeeMasterQueryRepository>();

            return services;
        }
    }
}