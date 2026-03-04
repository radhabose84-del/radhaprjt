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
using SalesManagement.Application.Common.Interfaces.IItemPriceMaster;
using SalesManagement.Infrastructure.Repositories.ItemPriceMaster;
using SalesManagement.Application.Common.Interfaces.IMiscTypeMaster;
using SalesManagement.Infrastructure.Repositories.MiscTypeMaster;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Infrastructure.Repositories.MiscMaster;
using SalesManagement.Application.Common.Interfaces.IAgentCommissionConfig;
using SalesManagement.Infrastructure.Repositories.AgentCommissionConfig;
using SalesManagement.Application.Common.Interfaces.IDispatchAddressMaster;
using SalesManagement.Infrastructure.Repositories.DispatchAddressMaster;
using SalesManagement.Application.Common.Interfaces.IMarketingOfficer;
using SalesManagement.Infrastructure.Repositories.MarketingOfficer;
using SalesManagement.Infrastructure.Services;
using Serilog;
using Microsoft.Extensions.Hosting;
using SalesManagement.Application.Common.Interfaces.IDispatchAddressMapping;
using SalesManagement.Infrastructure.Repositories.DispatchAddressMapping;
using SalesManagement.Application.Common.Interfaces.ISalesContact;
using SalesManagement.Infrastructure.Repositories.SalesContact;
using SalesManagement.Application.Common.Interfaces.ISalesLead;
using SalesManagement.Infrastructure.Repositories.SalesLead;
using SalesManagement.Application.Common.Interfaces.IOfficerAgent;
using SalesManagement.Infrastructure.Repositories.OfficerAgent;
using SalesManagement.Application.Common.Interfaces.ISalesEnquiry;
using SalesManagement.Infrastructure.Repositories.SalesEnquiry;
using SalesManagement.Application.Common.Interfaces.ISalesQuotation;
using SalesManagement.Infrastructure.Repositories.SalesQuotation;
using SalesManagement.Application.Common.Interfaces.ICustomerVisit;
using SalesManagement.Infrastructure.Repositories.CustomerVisit;
using SalesManagement.Application.Common.Interfaces.ISalesOrder;
using SalesManagement.Infrastructure.Repositories.SalesOrder;
using SalesManagement.Application.Common.Interfaces.ILotMaster;
using SalesManagement.Infrastructure.Repositories.LotMaster;
using SalesManagement.Application.Common.Interfaces.IPackType;
using SalesManagement.Infrastructure.Repositories.PackType;
using SalesManagement.Application.Common.Interfaces.IProduction;
using SalesManagement.Infrastructure.Repositories.Production;
using SalesManagement.Application.Common.Interfaces.IMovementTypeConfig;
using SalesManagement.Infrastructure.Repositories.MovementTypeConfig;


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
  			// ── Item Price Master Repositories ─────────────────────
            services.AddScoped<IItemPriceMasterCommandRepository, ItemPriceMasterCommandRepository>();
            services.AddScoped<IItemPriceMasterQueryRepository, ItemPriceMasterQueryRepository>();

            // ── Misc Type Master Repositories ─────────────────────────────
            services.AddScoped<IMiscTypeMasterCommandRepository, MiscTypeMasterCommandRepository>();
            services.AddScoped<IMiscTypeMasterQueryRepository, MiscTypeMasterQueryRepository>();

            // ── Misc Master Repositories ──────────────────────────────────
            services.AddScoped<IMiscMasterCommandRepository, MiscMasterCommandRepository>();
            services.AddScoped<IMiscMasterQueryRepository, MiscMasterQueryRepository>();

            // ── Agent Commission Configuration Repositories ─────────────
            services.AddScoped<IAgentCommissionConfigCommandRepository, AgentCommissionConfigCommandRepository>();
            services.AddScoped<IAgentCommissionConfigQueryRepository, AgentCommissionConfigQueryRepository>();
            // ── Dispatch Address Master Repositories ──────────────────────
            services.AddScoped<IDispatchAddressMasterCommandRepository, DispatchAddressMasterCommandRepository>();
            services.AddScoped<IDispatchAddressMasterQueryRepository, DispatchAddressMasterQueryRepository>();

            // ── Marketing Officer Repositories ──────────────────────────────
            services.AddScoped<IMarketingOfficerCommandRepository, MarketingOfficerCommandRepository>();
            services.AddScoped<IMarketingOfficerQueryRepository, MarketingOfficerQueryRepository>();
           // ── Dispatch Address Mapping Repositories ─────────────────────
            services.AddScoped<IDispatchAddressMappingCommandRepository, DispatchAddressMappingCommandRepository>();
            services.AddScoped<IDispatchAddressMappingQueryRepository, DispatchAddressMappingQueryRepository>();

            // ── Sales Contact Repositories ────────────────────────────────
            services.AddScoped<ISalesContactCommandRepository, SalesContactCommandRepository>();
            services.AddScoped<ISalesContactQueryRepository, SalesContactQueryRepository>();

            // ── Sales Lead Repositories ───────────────────────────────────
            services.AddScoped<ISalesLeadCommandRepository, SalesLeadCommandRepository>();
            services.AddScoped<ISalesLeadQueryRepository, SalesLeadQueryRepository>();

            // ── Officer Agent Repositories ────────────────────────────────
            services.AddScoped<IOfficerAgentCommandRepository, OfficerAgentCommandRepository>();
            services.AddScoped<IOfficerAgentQueryRepository, OfficerAgentQueryRepository>();

            // ── Sales Enquiry Repositories ──────────────────────────────
            services.AddScoped<ISalesEnquiryCommandRepository, SalesEnquiryCommandRepository>();
            services.AddScoped<ISalesEnquiryQueryRepository, SalesEnquiryQueryRepository>();

            // ── Sales Quotation Repositories ────────────────────────────
            services.AddScoped<ISalesQuotationCommandRepository, SalesQuotationCommandRepository>();
            services.AddScoped<ISalesQuotationQueryRepository, SalesQuotationQueryRepository>();

            // ── Customer Visit Repositories ─────────────────────────────
            services.AddScoped<ICustomerVisitCommandRepository, CustomerVisitCommandRepository>();
            services.AddScoped<ICustomerVisitQueryRepository, CustomerVisitQueryRepository>();

            // ── Sales Order Repositories ──────────────────────────────────
            services.AddScoped<ISalesOrderCommandRepository, SalesOrderCommandRepository>();
            services.AddScoped<ISalesOrderQueryRepository, SalesOrderQueryRepository>();

            // ── Lot Master Repositories ───────────────────────────────────
            services.AddScoped<ILotMasterCommandRepository, LotMasterCommandRepository>();
            services.AddScoped<ILotMasterQueryRepository, LotMasterQueryRepository>();

            // ── Pack Type Repositories ──────────────────────────────────────
            services.AddScoped<IPackTypeCommandRepository, PackTypeCommandRepository>();
            services.AddScoped<IPackTypeQueryRepository, PackTypeQueryRepository>();

            // ── Pack Allocation Repositories ────────────────────────────────
            services.AddScoped<IProductionCommandRepository, ProductionCommandRepository>();
            services.AddScoped<IProductionQueryRepository, ProductionQueryRepository>();

            // ── Movement Type Config Repositories ─────────────────────────────
            services.AddScoped<IMovementTypeConfigCommandRepository, MovementTypeConfigCommandRepository>();
            services.AddScoped<IMovementTypeConfigQueryRepository, MovementTypeConfigQueryRepository>();

            return services;
        }
    }
}