using System.Data;
using ProjectManagement.Application.Common.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Serilog;
using ProjectManagement.Infrastructure.Data;
using ProjectManagement.Infrastructure.Services;
using ProjectManagement.Application.Common.Interfaces.IMiscTypeMaster;
using ProjectManagement.Infrastructure.Repositories.MiscTypeMaster;
using ProjectManagement.Application.Common.Interfaces.IMiscMaster;
using ProjectManagement.Infrastructure.Repositories.MiscMaster;
using ProjectManagement.Application.Common.Mappings;
using ProjectManagement.Application.Common.Interfaces.IProjectMaster;
using ProjectManagement.Infrastructure.Repositories.ProjectMaster;
using ProjectManagement.Infrastructure.Repositories;
using ProjectManagement.Application.Common.Interfaces.IProjectWorkBreakdownStructure;
using ProjectManagement.Infrastructure.Repositories.ProjectWorkBreakdownStructure;
using Microsoft.Extensions.Hosting;
using Contracts.Interfaces.Lookups.Projects;
using ProjectManagement.Infrastructure.Repositories.Lookups.Projects;

namespace ProjectManagement.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddProjectInfrastructure(this IServiceCollection services, IConfiguration configuration, IHostEnvironment env)
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
            services.AddScoped<IMiscTypeMasterQueryRepository, MiscTypeMasterQueryRepository>();
            services.AddScoped<IMiscTypeMasterCommandRepository, MiscTypeMasterCommandRepository>();
            services.AddScoped<IMiscMasterQueryRepository, MiscMasterQueryRepository>();
            services.AddScoped<IMiscMasterCommandRepository, MiscMasterCommandRepository>();
            // services.AddScoped<IProjectMasterQueryRepository, ProjectMasterQueryRepository>();
            services.AddScoped<IProjectMasterCommandRepository, ProjectMasterCommandRepository>();
            services.AddScoped<IUploadDocumentQueryRepository, DocumentQueryRepository>();

            services.AddScoped<IProjectWorkBreakdownStructureQueryRepository, ProjectWorkBreakdownStructureQueryRepository>();
            services.AddScoped<IProjectWorkBreakdownStructureCommandRepository, ProjectWorkBreakdownStructureCommandRepository>();

            services.AddScoped<IProjectLookup, ProjectLookupRepository>();
            services.AddScoped<IProjectWbsLookup, ProjectWbsLookupRepository>();      
            // Miscellaneous services
            services.AddScoped<IIPAddressService, IPAddressService>();
            services.AddTransient<IFileUploadService, FileUploadRepository>();
            services.AddSingleton<ITimeZoneService, TimeZoneService>();
            services.AddTransient<IJwtTokenHelper, JwtTokenHelper>();
            services.AddScoped<ILogQueryService, LogQueryService>();


            // AutoMapper profiles
            services.AddAutoMapper(
             typeof(MiscTypeMasterProfile),
            typeof(MiscMasterProfile),
            typeof(ProjectMasterProfile),
            typeof(ProjectWorkBreakdownStructureProfile)


             );
            return services;
        }
    }
}