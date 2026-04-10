#nullable disable
using System.Data;
using PartyManagement.Application.Common.Interfaces;
using PartyManagement.Application.Common.Interfaces.AuditLog;
using PartyManagement.Application.Common.Interfaces.IBankAccount;
using PartyManagement.Application.Common.Interfaces.IBankMaster;
using PartyManagement.Application.Common.Interfaces.IMiscMaster;
using PartyManagement.Application.Common.Interfaces.IMiscTypeMaster;
using PartyManagement.Application.Common.Interfaces.IPartyGroup;
using PartyManagement.Application.Common.Interfaces.IPartyMaster;
using PartyManagement.Application.Common.Mappings;
using PartyManagement.Application.Interfaces.GST;
using Infrastructure.Data;
using Infrastructure.Repositories.Party.BankAccounts;
using InventoryManagement.Infrastructure.Services;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using PartyManagement.Infrastructure.Data;
using PartyManagement.Infrastructure.Repositories;
using PartyManagement.Infrastructure.Repositories.BankAccount;
using PartyManagement.Infrastructure.Repositories.BankMaster;
using PartyManagement.Infrastructure.Repositories.MiscMaster;
using PartyManagement.Infrastructure.Repositories.MiscTypeMaster;
using PartyManagement.Infrastructure.Repositories.PartyGroup;
using PartyManagement.Infrastructure.Repositories.PartyMaster;
using PartyManagement.Infrastructure.Repositories.Lookups;
using PartyManagement.Infrastructure.Repositories.Updates;
using PartyManagement.Infrastructure.Services;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Updates.Party;
using Serilog;
using Microsoft.Extensions.Hosting;

namespace PartyManagement.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPartyInfrastructure(this IServiceCollection services, IConfiguration configuration,IHostEnvironment env)
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
            services.AddSingleton<IMongoDatabase>(sp =>
            {
                var mongoDbContext = (MongoDbContext)sp.GetRequiredService<IMongoDbContext>();
                return mongoDbContext.GetDatabase();
            });

            // SQL Transactional Outbox
            services.AddScoped<Application.Common.Interfaces.IOutbox.IOutboxRepository, Repositories.Outbox.OutboxRepository>();
            services.AddScoped<Application.Common.Interfaces.IOutbox.IOutboxEventPublisher, Services.Outbox.OutboxEventPublisher>();
            services.AddScoped<Application.Common.Interfaces.IPartyUnitOfWork, Services.PartyUnitOfWork>();


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
                  // Register repositories
            services.AddScoped<IPartyGroupCommandRepository, PartyGroupCommandRepository>();
            services.AddScoped<IPartyGroupQueryRepository, PartyGroupQueryRepository>();
            services.AddScoped<IMiscTypeMasterCommandRepository, MiscTypeMasterCommandRepository>();
            services.AddScoped<IMiscTypeMasterQueryRepository, MiscTypeMasterQueryRepository>();
            services.AddScoped<IMiscMasterCommandRepository, MiscMasterCommandRepository>();
            services.AddScoped<IMiscMasterQueryRepository, MiscMasterQueryRepository>();
            services.AddScoped<IPartyMasterQueryRepository, PartyMasterQueryRepository>();
            services.AddScoped<IPartyMasterCommandRepository, PartyMasterCommandRepository>();
            services.AddScoped<IPartyActivityLogCommandRepository, PartyActivityLogCommandRepository>();
            services.AddScoped<IBankAccountCommandRepository, BankAccountCommandRepository>();
            services.AddScoped<IBankAccountQueryRepository, BankAccountQueryRepository>();
            services.AddScoped<IBankMasterQueryRepository, BankMasterQueryRepository>();
            services.AddScoped<IBankMasterCommandRepository, BankMasterCommandRepository>();

            // Lookups
            services.AddScoped<IPartyLookup, PartyLookupRepository>();
            services.AddScoped<ICustomerLookup, CustomerLookupRepository>();
            services.AddScoped<IAgentLookup, AgentLookupRepository>();
            services.AddScoped<ISubAgentLookup, SubAgentLookupRepository>();
            services.AddScoped<IPartyDetailLookup, PartyDetailLookupRepository>();
            services.AddScoped<IPartyBankLookup, PartyBankLookupRepository>();

            // Cross-module updates
            services.AddScoped<IPartyFreightUpdate, PartyFreightUpdateRepository>();

            // Miscellaneous services
            services.AddTransient<IFileUploadService, FileUploadRepository>();
            services.AddSingleton<ITimeZoneService, TimeZoneService>();
            services.AddTransient<IJwtTokenHelper, JwtTokenHelper>();
			services.AddHttpClient<IGSTAuthService, GSTAuthService>(); 

            // AutoMapper profiles
            services.AddAutoMapper(
            typeof(PartyGroupProfile),
            typeof(MiscTypeMasterProfile),
            typeof(MiscMasterProfile),
            typeof(PartyMasterProfile)

            );
            return services;
        }

    }
}
