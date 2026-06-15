using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using FinanceManagement.Application.Common.Interfaces;
using FinanceManagement.Application.Common.Interfaces.AuditLog;
using FinanceManagement.Application.Common.Interfaces.IAccountGroup;
using FinanceManagement.Application.Common.Interfaces.IDocumentSequence;
using FinanceManagement.Application.Common.Interfaces.IEInvoiceHeader;
using FinanceManagement.Application.Common.Interfaces.IEWaybillHeader;
using FinanceManagement.Application.Common.Interfaces.ITransactionTypeMaster;
using FinanceManagement.Application.Common.Interfaces.IMiscTypeMaster;
using FinanceManagement.Application.Common.Interfaces.IMiscMaster;
using FinanceManagement.Infrastructure.Data;
using FinanceManagement.Infrastructure.Persistence;
using FinanceManagement.Infrastructure.Repositories.AuditLog;
using Contracts.Interfaces.Lookups.Finance;
using FinanceManagement.Infrastructure.Repositories.AccountGroup;
using FinanceManagement.Infrastructure.Repositories.DocumentSequence;
using FinanceManagement.Infrastructure.Repositories.EInvoiceHeader;
using FinanceManagement.Infrastructure.Repositories.EWaybillHeader;
using FinanceManagement.Infrastructure.Repositories.Lookups.Finance;
using FinanceManagement.Infrastructure.Repositories.TransactionTypeMaster;
using FinanceManagement.Infrastructure.Repositories.MiscTypeMaster;
using FinanceManagement.Infrastructure.Repositories.MiscMaster;
using FinanceManagement.Infrastructure.Services;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using Shared.Infrastructure.Resilience;
using Serilog;

namespace FinanceManagement.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddFinanceInfrastructureServices(
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

            // ── Entity repositories ──────────────────────────────────────────
            services.AddScoped<ITransactionTypeMasterCommandRepository, TransactionTypeMasterCommandRepository>();
            services.AddScoped<ITransactionTypeMasterQueryRepository, TransactionTypeMasterQueryRepository>();

            services.AddScoped<IDocumentSequenceCommandRepository, DocumentSequenceCommandRepository>();
            services.AddScoped<IDocumentSequenceQueryRepository, DocumentSequenceQueryRepository>();

            services.AddScoped<IAccountGroupCommandRepository, AccountGroupCommandRepository>();
            services.AddScoped<IAccountGroupQueryRepository, AccountGroupQueryRepository>();

            services.AddScoped<IEInvoiceHeaderCommandRepository, EInvoiceHeaderCommandRepository>();
            services.AddScoped<IEInvoiceHeaderQueryRepository, EInvoiceHeaderQueryRepository>();

            services.AddScoped<IEWaybillHeaderCommandRepository, EWaybillHeaderCommandRepository>();
            services.AddScoped<IEWaybillHeaderQueryRepository, EWaybillHeaderQueryRepository>();

            services.AddScoped<IMiscTypeMasterCommandRepository, MiscTypeMasterCommandRepository>();
            services.AddScoped<IMiscTypeMasterQueryRepository, MiscTypeMasterQueryRepository>();

            services.AddScoped<IMiscMasterCommandRepository, MiscMasterCommandRepository>();
            services.AddScoped<IMiscMasterQueryRepository, MiscMasterQueryRepository>();

            // ── Lookup repositories (consumed by other modules via Contracts) ──
            services.AddScoped<IDocumentSequenceLookup, DocumentSequenceLookupRepository>();
            services.AddScoped<ITransactionTypeLookup, TransactionTypeLookupRepository>();
            services.AddScoped<IEInvoiceLookup, EInvoiceLookupRepository>();
            services.AddScoped<IEWaybillLookup, EWaybillLookupRepository>();

            // ── NIC E-Invoice service ─────────────────────────────────────────
            // Named HttpClient for NIC API calls; base address is set dynamically
            // inside NicEInvoiceService.GetConfig() so the client has no base address here.
            // SSL validation is intentionally bypassed for the NIC sandbox which uses
            // a self-signed certificate.
            services.AddHttpClient("NicEInvoice")
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                })
                .AddBsoftHttpResilience(ResilienceProfileNames.Critical);

            services.AddScoped<INicEInvoiceService, NicEInvoiceService>();

            // Validation repositories — cross-module referential integrity (Rule 25)
            services.AddScoped<Contracts.Interfaces.Validations.FinanceManagement.IPartyMasterFinanceValidation, Repositories.Validations.PartyMasterFinanceValidationRepository>();

            return services;
        }
    }
}
