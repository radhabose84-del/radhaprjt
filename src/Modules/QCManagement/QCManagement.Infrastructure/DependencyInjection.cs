using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using QCManagement.Application.Common.Interfaces;
using QCManagement.Application.Common.Interfaces.AuditLog;
using QCManagement.Infrastructure.Data;
using QCManagement.Infrastructure.Persistence;
using QCManagement.Infrastructure.Repositories.AuditLog;
using QCManagement.Infrastructure.Services;
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

            return services;
        }
    }
}
