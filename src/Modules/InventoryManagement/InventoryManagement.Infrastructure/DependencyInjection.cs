#nullable disable
using System.Data;
using InventoryManagement.Application.Common.Interfaces;
using InventoryManagement.Application.Common.Interfaces.AuditLog;
using InventoryManagement.Application.Common.Interfaces.Budget;
using InventoryManagement.Application.Common.Interfaces.IHSNMaster;
using InventoryManagement.Application.Common.Interfaces.IIssue;
using InventoryManagement.Application.Common.Interfaces.IMiscMaster;
using InventoryManagement.Application.Common.Interfaces.IMiscTypeMaster;
using InventoryManagement.Application.Common.Interfaces.IMRS;
using InventoryManagement.Application.Common.Interfaces.IReports.IStockReport;
using InventoryManagement.Application.Common.Interfaces.IStock;
using InventoryManagement.Application.Common.Interfaces.Item.ItemCategory;
using InventoryManagement.Application.Common.Interfaces.Item.ItemDetail.Commands;
using InventoryManagement.Application.Common.Interfaces.Item.ItemDetail.Queries;
using InventoryManagement.Application.Common.Interfaces.Item.ItemGroup;
using InventoryManagement.Application.Common.Interfaces.Item.PutAway;
using InventoryManagement.Application.Common.Interfaces.Item.Templates;
using InventoryManagement.Application.Common.Interfaces.IUOM;
using InventoryManagement.Application.Common.Interfaces.IUOMConversion;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetItemLogById;
using Infrastructure.Data;
using Infrastructure.Persistence.Repositories;
using InventoryManagement.Infrastructure.Data;
using InventoryManagement.Infrastructure.Data.Repositories.Item.Templates;
using InventoryManagement.Infrastructure.Repositories;
using InventoryManagement.Infrastructure.Repositories.HSNMaster;
using InventoryManagement.Infrastructure.Repositories.Issue;
using InventoryManagement.Infrastructure.Repositories.Item.ItemCategory;
using InventoryManagement.Infrastructure.Repositories.Item.ItemDetail.Commands;
using InventoryManagement.Infrastructure.Repositories.Item.ItemDetail.Queries;
using InventoryManagement.Infrastructure.Repositories.Item.ItemGroup;
using InventoryManagement.Infrastructure.Repositories.Item.Templates;
using InventoryManagement.Infrastructure.Repositories.MiscMaster;
using InventoryManagement.Infrastructure.Repositories.MiscTypeMaster;
using InventoryManagement.Infrastructure.Repositories.MRS;
using InventoryManagement.Infrastructure.Repositories.Reports;
using InventoryManagement.Infrastructure.Repositories.Stock;
using InventoryManagement.Infrastructure.Repositories.UOMConversion;
using InventoryManagement.Infrastructure.Repositories.UOMs;
using InventoryManagement.Infrastructure.Repositories.Lookups;
using InventoryManagement.Infrastructure.Services;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Workflow;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Serilog;
using Microsoft.Extensions.Hosting;

namespace InventoryManagement.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInventoryInfrastructure(this IServiceCollection services, IConfiguration configuration, IHostEnvironment env)
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
            services.AddScoped<IAuditLogRepository, AuditLogRepository>();
            services.AddScoped<IItemGroupCommandRepository, ItemGroupCommandRepository>();
            services.AddScoped<IItemGroupQueryRepository, ItemGroupQueryRepository>();
            services.AddScoped<IItemCategoryQueryRepository, ItemCategoryQueryRepository>();
            services.AddScoped<IItemCategoryCommandRepository, ItemCategoryCommandRepository>();
            services.AddScoped<IMiscTypeMasterQueryRepository, MiscTypeMasterQueryRepository>();
            services.AddScoped<IMiscTypeMasterCommandRepository, MiscTypeMasterCommandRepository>();
            services.AddScoped<IMiscMasterQueryRepository, MiscMasterQueryRepository>();

            services.AddScoped<IMiscMasterCommandRepository, MiscMasterCommandRepository>();
            services.AddScoped<IHSNMasterQueryRepository, HSNMasterQueryRepository>();
            services.AddScoped<IHSNMasterCommandRepository, HSNMasterCommandRepository>();
            services.AddScoped<IUOMQueryRepository, UOMQueryRepository>();
            services.AddScoped<IUOMCommandRepository, UOMCommandRepository>();
            services.AddScoped<IUOMConversionQueryRepository, UOMConversionQueryRepository>();
            services.AddScoped<IUOMConversionCommandRepository, UOMConversionCommandRepository>();
            services.AddScoped<IBudgetCommandRepository, BudgetCommandRepository>();
            services.AddScoped<IBudgetQueryRepository, BudgetQueryRepository>();
            services.AddScoped<IBudgetLogQueryRepository, BudgetLogQueryRepository>();
            //Item master
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IItemCommandRepository, ItemCommandRepository>();
            services.AddScoped<IItemPurchaseCommandRepository, ItemPurchaseCommandRepository>();
            services.AddScoped<IItemInventoryCommandRepository, ItemInventoryCommandRepository>();
            services.AddScoped<IItemQualityCommandRepository, ItemQualityCommandRepository>();
            services.AddScoped<IItemSaleCommandRepository, ItemSaleCommandRepository>();
            services.AddScoped<IItemUomCommandRepository, ItemUomCommandRepository>();
            services.AddScoped<IItemManufactureCommandRepository, ItemManufactureCommandRepository>();
            services.AddScoped<IItemSupplierCommandRepository, ItemSupplierCommandRepository>();
            services.AddScoped<IItemVariantValueCommandRepository, ItemVariantValueCommandRepository>();
            services.AddScoped<IItemVariantValueQueryRepository, ItemVariantValueQueryRepository>();
            services.AddScoped<IItemQueryRepository, ItemQueryRepository>();
            services.AddScoped<ITemplateQueryRepository, TemplateQueryRepository>();
            services.AddScoped<ITemplateCommandRepository, TemplateCommandRepository>();
            services.AddScoped<IPutAwayRuleCommandRepository, PutAwayRuleCommandRepository>();
            services.AddScoped<IPutAwayRuleQueryRepository, PutAwayRuleQueryRepository>();
            services.AddScoped<IItemVariantAttributeCommandRepository, ItemVariantAttributeCommandRepository>();
            services.AddScoped<IItemLogQueryRepository, ItemLogQueryRepository>();
            services.AddScoped<IStockLedgerRepository, StockLedgerRepository>();
            services.AddScoped<IMrsEntryCommandRepository, MrsEntryCommandRepository>();
            services.AddScoped<IMrsEntryQueryRepository, MrsEntryQueryRepository>();
            services.AddScoped<IIssueEntryCommandRepository, IssueEntryCommandRepository>();
            services.AddScoped<IIssueQueryCommandRepository, IssueEntryQueryRepository>();
            services.AddScoped<IStockReportQueryRepository, StockReportQueryRepository>();

            // Lookups
            services.AddScoped<IUOMLookup, UOMLookupRepository>();
            services.AddScoped<IItemLookup, ItemLookupRepository>();
            services.AddScoped<IMiscMasterLookup, MiscMasterLookupRepository>();
            services.AddScoped<IHSNLookup, HSNLookupRepository>();
            services.AddScoped<IWorkflowLookup, WorkflowLookupRepository>();
            services.AddScoped<IItemPurchaseToleranceLookup, ItemPurchaseToleranceLookupRepository>();
            services.AddScoped<IPutawayRuleLookup, PutawayRuleLookupRepository>();
            services.AddScoped<IInventoryCategoryLookup, ItemCategoryLookupRepository>();

            // Miscellaneous services
            services.AddTransient<IFileUploadService, FileUploadRepository>();
            services.AddSingleton<ITimeZoneService, TimeZoneService>();
            services.AddTransient<IJwtTokenHelper, JwtTokenHelper>();

            // AutoMapper profiles
            // services.AddAutoMapper(
            //        typeof(MiscTypeMasterProfile),

            //       typeof(MiscMasterProfile),
            //       typeof(HSNMasterProfile),
            //       typeof(UOMProfile),
            //       typeof(UOMConversionProfile),
            //       typeof(ItemProfile),
            //       typeof(MrsEntryProfile),
            //       typeof(IssueEntryProfile)




            //   );
            return services;
        }

    }
}

