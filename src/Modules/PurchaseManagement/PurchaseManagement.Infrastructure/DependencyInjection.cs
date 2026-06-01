#nullable disable
using System.Data;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.AuditLog;
using PurchaseManagement.Application.Common.Interfaces.ILogService;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.Common.Interfaces.IMiscTypeMaster;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseIndent;
using PurchaseManagement.Application.Common.Interfaces.IPartyMaster;
using Infrastructure.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using PurchaseManagement.Infrastructure.Data;
using PurchaseManagement.Infrastructure.Repositories;
using PurchaseManagement.Infrastructure.Repositories.LogServices;
using PurchaseManagement.Infrastructure.Repositories.MiscMaster;
using PurchaseManagement.Infrastructure.Repositories.MiscTypeMaster;
using PurchaseManagement.Infrastructure.Repositories.PurchaseIndents;
using PurchaseManagement.Infrastructure.Repositories.PartyMaster;
using PurchaseManagement.Infrastructure.Services;
using Serilog;
using Shared.Infrastructure.Resilience;
using Contracts.Interfaces.Lookups.Workflow;
using PurchaseManagement.Application.Common.Interfaces.IPaymentTermMaster;
using PurchaseManagement.Infrastructure.Repositories.PaymentTermMaster;
using PurchaseManagement.Application.Common.Interfaces.ITnCTemplateMaster;
using PurchaseManagement.Infrastructure.Repositories.TncTemplateMaster;
using PurchaseManagement.Application.Common.Interfaces.IQuotation.IQuotationEntry;
using PurchaseManagement.Application.Common.Interfaces.IQuotation.IQuotationCompare;
using PurchaseManagement.Infrastructure.Repositories.Quotation.QuotationCompare;
using PurchaseManagement.Application.Common.Interfaces.PriceMaster;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.Local;
using PurchaseManagement.Infrastructure.Repositories.PurchaseOrder.Local;
using PurchaseManagement.Application.Common.Interfaces.IGRN.IGateEntry;
using PurchaseManagement.Infrastructure.Repositories.GRN.GateEntry;
using PurchaseManagement.Infrastructure.Repositories.PriceMaster;
using PurchaseManagement.Application.Common.Interfaces.IGRN.IGRNEntry;
using PurchaseManagement.Infrastructure.Repositories.GRN.GRNEntry;
using System.Net;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder;
using PurchaseManagement.Infrastructure.Repositories.Reporting;
using PurchaseManagement.Infrastructure.Files;
using PurchaseManagement.Application.Common.Interfaces.IServiceMaster;
using PurchaseManagement.Infrastructure.Repositories.ServiceMaster;
using PurchaseManagement.Application.Common.Interfaces.IReports.IStockReport;
using PurchaseManagement.Infrastructure.Repositories.Reports.StockReport;
using PurchaseManagement.Application.Common.Interfaces.IMRS;
using PurchaseManagement.Infrastructure.Repositories.MRS;
using PurchaseManagement.Application.Common.Interfaces.IIssue;
using PurchaseManagement.Infrastructure.Repositories.Issue;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ServicePO;
using PurchaseManagement.Application.External;
using PurchaseManagement.Application.ExchangeRate.Interfaces;
using PurchaseManagement.Infrastructure.Repositories.ExchangeRate;
using PurchaseManagement.Application.Common.Interfaces.IExchangeRate;
using PurchaseManagement.Application.Common.Interfaces.IPortMaster;
using PurchaseManagement.Infrastructure.Repositories.Port;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IContractPOMaster;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ImportPO;
using PurchaseManagement.Application.Common.Interfaces.IPurchase.DutyMaster;
using PurchaseManagement.Infrastructure.Repositories.DutyMaster;
//using PurchaseManagement.Infrastructure.Repositories.Lookups.Workflow;
using PurchaseManagement.Application.Common.Interfaces.IDutyMaster;
using PurchaseManagement.Application.Common.Interfaces.IPoMethodLookup;
using PurchaseManagement.Infrastructure.Repositories.PoMethodLookup;
using PurchaseManagement.Application.Common.Interfaces.IIssueReturn;
using PurchaseManagement.Infrastructure.Repositories.IssueReturn;
using PurchaseManagement.Infrastructure.Repositories.PurchaseOrder;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IPurchaseDocument;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IBillEntry;
using PurchaseManagement.Infrastructure.Repositories.PurchaseOrder.BillEntry;
using Microsoft.Extensions.Hosting;
using PurchaseManagement.Application.Common.Interfaces.IQuotation.IRfqEntry;
using PurchaseManagement.Infrastructure.Repositories.Quotation.QuotationEntry;
using PurchaseManagement.Infrastructure.Repositories.Quotation.RfqEntry;
using PurchaseManagement.Infrastructure.PurchaseOrder.Local;
using PurchaseManagement.Infrastructure.Repositories.PurchaseOrder.ImportPO;
using PurchaseManagement.Infrastructure.Repositories.PurchaseOrder.ServicePO;
using PurchaseManagement.Infrastructure.Persistence;
using PurchaseManagement.Application.Common.Interfaces.IOutbox;
using PurchaseManagement.Infrastructure.Repositories.Outbox;
using PurchaseManagement.Infrastructure.Services.Outbox;

using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Purchase;
using PurchaseManagement.Infrastructure.Repositories.Lookups;
using PurchaseManagement.Infrastructure.Repositories.Lookups.Purchase;
using PurchaseManagement.Infrastructure.Repositories.ContractPOMaster;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IContractPO;
using PurchaseManagement.Infrastructure.Repositories.PurchaseOrder.ContractPO;
using PurchaseManagement.Application.Common.Interfaces.IBlanketMaster;
using PurchaseManagement.Infrastructure.Repositories.BlanketMaster;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IBlanketPO;
using PurchaseManagement.Infrastructure.Repositories.PurchaseOrder.BlanketPO;
using PurchaseManagement.Infrastructure.Repositories.PurchaseOrder.Print;
using PurchaseManagement.Application.Common.Interfaces.IVendorEvaluationCriteria;
using PurchaseManagement.Application.Common.Interfaces.IVendorRatingGrade;
using PurchaseManagement.Application.Common.Interfaces.IDeliveryScoreRule;
using PurchaseManagement.Application.Common.Interfaces.IVendorEvaluationHeader;
using PurchaseManagement.Infrastructure.Repositories.VendorEvaluationCriteria;
using PurchaseManagement.Infrastructure.Repositories.VendorRatingGrade;
using PurchaseManagement.Infrastructure.Repositories.DeliveryScoreRule;
using PurchaseManagement.Infrastructure.Repositories.VendorEvaluationHeader;
using PurchaseManagement.Application.Common.Interfaces.IReturnType;
using PurchaseManagement.Application.Common.Interfaces.IReturnReason;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseReturn;
using PurchaseManagement.Infrastructure.Repositories.PurchaseReturn;



namespace PurchaseManagement.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPurchaseInfrastructureServices(this IServiceCollection services, IConfiguration configuration, IHostEnvironment env)
        {

            var connectionString = configuration.GetConnectionString("DefaultConnection")
                                                .Replace("{SERVER}", Environment.GetEnvironmentVariable("DATABASE_SERVER") ?? "")
                                                .Replace("{USER_ID}", Environment.GetEnvironmentVariable("DATABASE_USERID") ?? "")
                                                .Replace("{ENC_PASSWORD}", Environment.GetEnvironmentVariable("DATABASE_PASSWORD") ?? "");


            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found or is empty.");
            }
            services.AddScoped<ActivityLogSaveChangesInterceptor>();
            // Register ApplicationDbContext with SQL Server
            services.AddDbContext<ApplicationDbContext>((sp, options) =>
           {
               options.UseSqlServer(connectionString);
               options.AddInterceptors(sp.GetRequiredService<ActivityLogSaveChangesInterceptor>());
           });

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

            // Register the OutboxMessage collection
            services.AddScoped<IMongoCollection<OutboxMessage>>(sp =>
            {
                var database = sp.GetRequiredService<IMongoDatabase>();
                var collectionName = configuration["MongoDbSettings:OutboxCollectionName"] ?? "OutboxMessages";
                return database.GetCollection<OutboxMessage>(collectionName);
            });



            // Register repositories
            services.AddScoped<IAuditLogRepository, AuditLogRepository>();
            services.AddScoped<IMiscTypeMasterQueryRepository, MiscTypeMasterQueryRepository>();
            services.AddScoped<IMiscTypeMasterCommandRepository, MiscTypeMasterCommandRepository>();
            services.AddScoped<IMiscMasterQueryRepository, MiscMasterQueryRepository>();
            services.AddScoped<IMiscMasterCommandRepository, MiscMasterCommandRepository>();

            // Pending reference-document resolvers consumed by GateEntryManagement.GateInward.
            // One implementation per Finance.TransactionTypeMaster.Id within ModuleId = 21 (Purchase).
            services.AddScoped<Contracts.Interfaces.Gate.IPendingReferenceDocResolver, Repositories.GateInward.LocalPoPendingResolver>();
            services.AddScoped<Contracts.Interfaces.Gate.IPendingReferenceDocResolver, Repositories.GateInward.ImportPoPendingResolver>();

            // Cross-module bridge — Gate Inward saves trigger GRN creation through the existing
            // CreateGRNEntryCommand pipeline (no changes to GRN module). Pre-validation runs
            // before Gate persists, so a tolerance breach aborts the Gate save.
            services.AddScoped<Contracts.Interfaces.Purchase.IGateInwardGrnBridge, Services.GateInwardGrnBridge>();
            services.AddScoped<IPurchaseIndentCommand, PurchaseIndentCommandRepository>();
            services.AddScoped<ILogServiceCommand, LogServiceCommandRepository>();
            services.AddScoped<IPurchaseIndentQuery, PurchaseIndentQueryRepository>();
            services.AddScoped<IPartyMasterQueryRepository, PartyMasterQueryRepository>();
            services.AddScoped<IPaymentTermMasterQueryRepository, PaymentTermMasterQueryRepository>();
            services.AddScoped<IPaymentTermMasterCommandRepository, PaymentTermMasterCommandRepository>();
            //Rfq
            services.AddScoped<IRfqCommandRepository, RfqCommandRepository>();
            services.AddScoped<IRfqQueryRepository, RfqQueryRepository>();
            services.AddScoped<IRfqAttachmentFileStorage, RfqAttachmentFileStorage>();
            //QuotationEntry
            services.AddScoped<IQuotationCommandRepository, QuotationCommandRepository>();
            services.AddScoped<IQuotationQueryRepository, QuotationQueryRepository>();
            services.AddScoped<IQuotationCompareQueryRepository, QuotationCompareQueryRepository>();
            services.AddScoped<IQuotationCompareCommandRepository, QuotationCompareCommandRepository>();


            services.AddScoped<ITnCTemplateMasterQueryRepository, TncTemplateMasterQueryRepository>();
            services.AddScoped<ITnCTemplateMasterCommandRepository, TncTemplateMasterCommandRepository>();
            services.AddScoped<ITnCTemplateCodeGenerator, TnCTemplateCodeGenerator>();

            services.AddScoped<IActivityLogQueryRepository, ActivityLogQueryRepository>();
            services.AddScoped<IPriceMasterCommandRepository, PriceMasterCommandRepository>();
            services.AddScoped<IPriceMasterQueryRepository, PriceMasterQueryRepository>();
            services.AddScoped<IGateEntryCommandRepository, GateEntryCommandRepository>();
            services.AddScoped<IGateEntryQueryRepository, GateEntryQueryRepository>();

            services.AddScoped<IPurchaseOrderCommandRepository, PurchaseOrderCommandRepository>();
            services.AddScoped<IPurchaseOrderQueryRepository, PurchaseOrderQueryRepository>();
            services.AddScoped<IGRNEntryCommandRepository, GRNEntryCommandRepository>();
            services.AddScoped<IGRNEntryQueryRepository, GRNEntryQueryRepository>();

            services.AddScoped<IServiceQueryRepository, ServiceQueryRepository>();
            services.AddScoped<IServiceCommandRepository, ServiceCommandRepository>();

            services.AddScoped<IStockReportQueryRepository, StockReportQueryRepository>();
            services.AddScoped<IMrsEntryCommandRepository, MrsEntryCommandRepository>();
            services.AddScoped<IMrsEntryQueryRepository, MrsEntryQueryRepository>();


            services.AddScoped<IIssueEntryCommandRepository, IssueEntryCommandRepository>();
            services.AddScoped<IIssueQueryCommandRepository, IssueEntryQueryRepository>();


            services.AddScoped<IServicePurchaseOrderCommandRepository, ServicePurchaseOrderCommandRepository>();
            services.AddScoped<IServicePurchaseOrderQueryRepository, ServicePurchaseOrderQueryRepository>();

            services.AddScoped<IImportPOQueryRepository, ImportPOQueryRepository>();
            services.AddScoped<IImportPOCommandRepository, ImportPOCommandRepository>();
            services.AddScoped<IDutyMasterQueryRepository, DutyMasterQueryRepository>();
            services.AddScoped<IDutyMasterCommandRepository, DutyMasterCommandRepository>();
          //  services.AddScoped<IWorkflowLookup, WorkflowLookupRepository>();
            // StockLedger ownership moved to InventoryManagement (Pattern D: Shared Transaction)
            // services.AddScoped<IStockLedgerLookup, StockLedgerLookupRepository>();
            services.AddScoped<IPaymentTermLookup, PaymentTermLookupRepository>();
            services.AddScoped<IIncotermLookup, IncotermLookupRepository>();

            services.AddScoped<IPoMethodLookup, PoMethodLookup>();
            services.AddScoped<IPODocumentQueryRepository, PODocumentQueryRepository>();



            // Miscellaneous services
            services.AddTransient<IFileUploadService, FileUploadRepository>();
            services.AddSingleton<ITimeZoneService, TimeZoneService>();
            services.AddTransient<IJwtTokenHelper, JwtTokenHelper>();
            services.AddSingleton<IFileStorage, FileStorage>();
            // ssrs
            services.Configure<SsrsOptions>(configuration.GetSection("Ssrs"));


            services.AddHttpClient<ISsrsClient, SsrsClient>()
                .ConfigurePrimaryHttpMessageHandler(sp =>
                {
                    var opt = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<SsrsOptions>>().Value;

                    var h = new HttpClientHandler
                    {
                        AllowAutoRedirect = true,
                        PreAuthenticate = true,
                        UseDefaultCredentials = opt.Auth.UseDefaultCredentials
                    };

                    if (!opt.Auth.UseDefaultCredentials)
                    {
                        h.Credentials = string.IsNullOrWhiteSpace(opt.Auth.Domain)
                            ? new NetworkCredential(opt.Auth.Username, opt.Auth.Password)
                            : new NetworkCredential(opt.Auth.Username, opt.Auth.Password, opt.Auth.Domain);
                    }

                    return h;
                })
                .AddBsoftHttpResilience(ResilienceProfileNames.Standard);
            services.AddHttpClient<IFrankfurterClient, FrankfurterClient>()
                .AddBsoftHttpResilience(ResilienceProfileNames.FastFail);
            services.AddScoped<IExchangeRateCommandRepository, ExchangeRateCommandRepository>();
            services.AddScoped<IExchangeRateQueryRepository, ExchangeRateQueryRepository>();


            services.AddScoped<IPortMasterQueryRepository, PortMasterQueryRepository>();
            services.AddScoped<IPortMasterCommandRepository, PortMasterCommandRepository>();

            // Purchase Return (RTV)
            services.AddScoped<IReturnTypeCommandRepository, ReturnTypeCommandRepository>();
            services.AddScoped<IReturnTypeQueryRepository, ReturnTypeQueryRepository>();
            services.AddScoped<IReturnReasonCommandRepository, ReturnReasonCommandRepository>();
            services.AddScoped<IReturnReasonQueryRepository, ReturnReasonQueryRepository>();
            services.AddScoped<IPurchaseReturnCommandRepository, PurchaseReturnCommandRepository>();
            services.AddScoped<IPurchaseReturnQueryRepository, PurchaseReturnQueryRepository>();

            services.AddScoped<IIssueReturnEntryCommandRepository, IssueReturnEntryCommandRepository>();
            services.AddScoped<IIssueReturnEntryQueryRepository, IssueReturnEntryQueryRepository>();

            services.AddScoped<IPurchaseBillEntryCommandRepository, PurchaseBillEntryCommandRepository>();
            services.AddScoped<IPurchaseBillEntryQueryRepository, PurchaseBillEntryQueryRepository>();

            services.AddScoped<IContractPOMasterCommandRepository, ContractPOMasterCommandRepository>();
            services.AddScoped<IContractPOMasterQueryRepository, ContractPOMasterQueryRepository>();
            services.AddScoped<IContractPOCommandRepository, ContractPOCommandRepository>();
            services.AddScoped<IContractPOQueryRepository, ContractPOQueryRepository>();

            // Blanket Master
            services.AddScoped<IBlanketMasterCommandRepository, BlanketMasterCommandRepository>();
            services.AddScoped<IBlanketMasterQueryRepository, BlanketMasterQueryRepository>();
            // Blanket Release PO
            services.AddScoped<IBlanketPOCommandRepository, BlanketPOCommandRepository>();
            services.AddScoped<IBlanketPOQueryRepository, BlanketPOQueryRepository>();

            // PO Print
            services.AddScoped<IPurchaseOrderPrintQueryRepository, PurchaseOrderPrintQueryRepository>();

            // Vendor Evaluation & Rating
            services.AddScoped<IVendorEvaluationCriteriaCommandRepository, VendorEvaluationCriteriaCommandRepository>();
            services.AddScoped<IVendorEvaluationCriteriaQueryRepository, VendorEvaluationCriteriaQueryRepository>();

            services.AddScoped<IVendorRatingGradeCommandRepository, VendorRatingGradeCommandRepository>();
            services.AddScoped<IVendorRatingGradeQueryRepository, VendorRatingGradeQueryRepository>();

            services.AddScoped<IDeliveryScoreRuleCommandRepository, DeliveryScoreRuleCommandRepository>();
            services.AddScoped<IDeliveryScoreRuleQueryRepository, DeliveryScoreRuleQueryRepository>();

            services.AddScoped<IVendorEvaluationHeaderCommandRepository, VendorEvaluationHeaderCommandRepository>();
            services.AddScoped<IVendorEvaluationHeaderQueryRepository, VendorEvaluationHeaderQueryRepository>();
            services.AddScoped<IVendorEvaluationDashboardQueryRepository, VendorEvaluationDashboardQueryRepository>();

            // Vendor Rating Lookup (cross-module)
            services.AddScoped<IVendorRatingLookup, VendorRatingLookupRepository>();

            // // AutoMapper profiles
            // services.AddAutoMapper(
            //     typeof(MiscTypeMasterProfile),
            //     typeof(MiscMasterProfile),
            //     typeof(PaymentTermProfile),
            //     typeof(TnCTemplateMasterProfile),
            //     typeof(QuotationCompareMappingProfile),
            //     typeof(GateEntryProfile),
            //     typeof(GRNEntryProfile),
            //     typeof(RfqMappingProfile),
            //     typeof(IssueEntryProfile),
            //     typeof (MrsEntryProfile),
            //     typeof(RfqMappingProfile),
            //     typeof(PurchaseOrderServiceProfile),
            //     typeof(IssueReturnProfile)







            // );

            // ═══════════════════════════════════════════════════════════════
            // OUTBOX PATTERN SERVICES (SQL-based for transaction atomicity)
            // ═══════════════════════════════════════════════════════════════

            // Outbox repository (SQL-based for transaction atomicity)
            services.AddScoped<IOutboxRepository, OutboxRepository>();

            // Outbox event publisher (saves events to outbox table)
            services.AddScoped<IOutboxEventPublisher, OutboxEventPublisher>();
            services.AddScoped<IPurchaseUnitOfWork, PurchaseUnitOfWork>();

            services.AddScoped<IEventPublisher, EventPublisher>();

            // Validation repositories — cross-module referential integrity (Rule 25)
            services.AddScoped<Contracts.Interfaces.Validations.PurchaseManagement.IPurchaseCurrencyValidation, Repositories.Validations.PurchaseCurrencyValidationRepository>();
            services.AddScoped<Contracts.Interfaces.Validations.PurchaseManagement.IPartyMasterPurchaseValidation, Repositories.Validations.PartyMasterPurchaseValidationRepository>();
            services.AddScoped<Contracts.Interfaces.Validations.PurchaseManagement.IPurchaseItemValidation, Repositories.Validations.PurchaseItemValidationRepository>();
            services.AddScoped<Contracts.Interfaces.Validations.PurchaseManagement.IPurchaseUomValidation, Repositories.Validations.PurchaseUomValidationRepository>();
            services.AddScoped<Contracts.Interfaces.Validations.PurchaseManagement.IPurchaseHsnValidation, Repositories.Validations.PurchaseHsnValidationRepository>();
            services.AddScoped<Contracts.Interfaces.Validations.PurchaseManagement.IPurchaseItemCategoryValidation, Repositories.Validations.PurchaseItemCategoryValidationRepository>();

            return services;
        }

    }
}

