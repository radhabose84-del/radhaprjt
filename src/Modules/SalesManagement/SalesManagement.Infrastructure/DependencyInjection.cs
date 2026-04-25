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
using SalesManagement.Application.Common.Interfaces.ISalesOrderAmendment;
using SalesManagement.Infrastructure.Repositories.SalesOrderAmendment;
using SalesManagement.Application.Common.Interfaces.IMovementTypeConfig;
using SalesManagement.Infrastructure.Repositories.MovementTypeConfig;
using SalesManagement.Application.Common.Interfaces.IDispatchAdvice;
using SalesManagement.Infrastructure.Repositories.DispatchAdvice;
using SalesManagement.Application.Common.Interfaces.IProformaInvoice;
using SalesManagement.Infrastructure.Repositories.ProformaInvoice;
using SalesManagement.Application.Common.Interfaces.IStoTypeMaster;
using SalesManagement.Infrastructure.Repositories.StoTypeMaster;
using SalesManagement.Application.Common.Interfaces.IStoHeader;
using SalesManagement.Infrastructure.Repositories.StoHeader;
using SalesManagement.Application.Common.Interfaces.IDeliveryChallan;
using SalesManagement.Infrastructure.Repositories.DeliveryChallan;
using SalesManagement.Application.Common.Interfaces.IInvoice;
using SalesManagement.Infrastructure.Repositories.Invoice;
using SalesManagement.Application.Common.Interfaces.IStoReceipt;
using SalesManagement.Infrastructure.Repositories.StoReceipt;

using SalesManagement.Application.Common.Interfaces.IStockLedger;
using SalesManagement.Infrastructure.Repositories.Reports.StockLedger;
using SalesManagement.Application.Common.Interfaces.ILeadConversionFunnel;
using SalesManagement.Infrastructure.Repositories.Reports.LeadConversionFunnel;
using SalesManagement.Application.Common.Interfaces.IAgentCustomerMapping;
using SalesManagement.Infrastructure.Repositories.AgentCustomerMapping;
using SalesManagement.Application.Common.Interfaces.IOutbox;
using SalesManagement.Infrastructure.Repositories.Outbox;
using SalesManagement.Infrastructure.Services.Outbox;
using SalesManagement.Application.Common.Interfaces.ITripSheet;
using SalesManagement.Infrastructure.Repositories.TripSheet;
using SalesManagement.Application.Common.Interfaces.IComplaint;
using SalesManagement.Infrastructure.Repositories.Complaint;
using SalesManagement.Application.Common.Interfaces.IComplaintQCReview;
using SalesManagement.Infrastructure.Repositories.ComplaintQCReview;
using SalesManagement.Application.Common.Interfaces.IComplaintDepartmentFeedback;
using SalesManagement.Application.Common.Interfaces.IComplaintResolution;
using SalesManagement.Application.Common.Interfaces.ISalesReturn;
using SalesManagement.Infrastructure.Repositories.SalesReturn;
using SalesManagement.Application.Common.Interfaces.IAgentPortal;
using SalesManagement.Infrastructure.Repositories.AgentPortal;
using SalesManagement.Application.Common.Interfaces.IDiscountMaster;
using SalesManagement.Infrastructure.Repositories.DiscountMaster;
using SalesManagement.Application.Common.Interfaces.ICommissionSplit;
using SalesManagement.Infrastructure.Repositories.CommissionSplit;
using SalesManagement.Infrastructure.Repositories.ComplaintDepartmentFeedback;
using SalesManagement.Infrastructure.Repositories.ComplaintResolution;
using Contracts.Interfaces.Lookups.Sales;
using SalesManagement.Infrastructure.Repositories.Lookups.Sales;

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

            // ── Sales Segment Lookup (cross-module) ────────────────────
            services.AddScoped<ISalesSegmentLookup, SalesSegmentLookupRepository>();

            // ── Sales Group Lookup (cross-module) ───────────────────────
            services.AddScoped<ISalesGroupLookup, SalesGroupLookupRepository>();

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

            // ── Agent Customer Mapping Repositories ──────────────────────
            services.AddScoped<IAgentCustomerMappingCommandRepository, AgentCustomerMappingCommandRepository>();
            services.AddScoped<IAgentCustomerMappingQueryRepository, AgentCustomerMappingQueryRepository>();
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

            // ── Marketing Officer Access Filter (per-request scoped cache) ─
            services.AddScoped<IMarketingOfficerAccessFilter, MarketingOfficerAccessFilter>();

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

            // ── Sales Order Amendment Repositories ────────────────────────
            services.AddScoped<ISalesOrderAmendmentCommandRepository, SalesOrderAmendmentCommandRepository>();
            services.AddScoped<ISalesOrderAmendmentQueryRepository, SalesOrderAmendmentQueryRepository>();

            // ── Movement Type Config Repositories ─────────────────────────────
            services.AddScoped<IMovementTypeConfigCommandRepository, MovementTypeConfigCommandRepository>();
            services.AddScoped<IMovementTypeConfigQueryRepository, MovementTypeConfigQueryRepository>();

            // ── Dispatch Advice Repositories ────────────────────────────────
            services.AddScoped<IDispatchAdviceCommandRepository, DispatchAdviceCommandRepository>();
            services.AddScoped<IDispatchAdviceQueryRepository, DispatchAdviceQueryRepository>();

            // ── Proforma Invoice Repositories ─────────────────────────────────
            services.AddScoped<IProformaInvoiceCommandRepository, ProformaInvoiceCommandRepository>();
            services.AddScoped<IProformaInvoiceQueryRepository, ProformaInvoiceQueryRepository>();

            // ── STO Type Master Repositories ────────────────────────────────
            services.AddScoped<IStoTypeMasterCommandRepository, StoTypeMasterCommandRepository>();
            services.AddScoped<IStoTypeMasterQueryRepository, StoTypeMasterQueryRepository>();

            // ── STO Header Repositories ─────────────────────────────────────
            services.AddScoped<IStoHeaderCommandRepository, StoHeaderCommandRepository>();
            services.AddScoped<IStoHeaderQueryRepository, StoHeaderQueryRepository>();

            // ── Delivery Challan Repositories ─────────────────────────────────
            services.AddScoped<IDeliveryChallanCommandRepository, DeliveryChallanCommandRepository>();
            services.AddScoped<IDeliveryChallanQueryRepository, DeliveryChallanQueryRepository>();

           // ── Invoice Repositories ───────────────────────────────────────────
            services.AddScoped<IInvoiceCommandRepository, InvoiceCommandRepository>();
            services.AddScoped<IInvoiceQueryRepository, InvoiceQueryRepository>();

            // ── STO Receipt Repositories ────────────────────────────────────────
            services.AddScoped<IStoReceiptCommandRepository, StoReceiptCommandRepository>();
            services.AddScoped<IStoReceiptQueryRepository, StoReceiptQueryRepository>();

            // ── Complaint Repositories ─────────────────────────────────────────────
            services.AddScoped<IComplaintCommandRepository, ComplaintCommandRepository>();
            services.AddScoped<IComplaintQueryRepository, ComplaintQueryRepository>();

            // ── Complaint QC Review Repositories ─────────────────────────────────
            services.AddScoped<IComplaintQCReviewCommandRepository, ComplaintQCReviewCommandRepository>();
            services.AddScoped<IComplaintQCReviewQueryRepository, ComplaintQCReviewQueryRepository>();

            // ── Complaint Department Feedback Repositories ──────────────────────
            services.AddScoped<IComplaintDepartmentFeedbackCommandRepository, ComplaintDepartmentFeedbackCommandRepository>();
            services.AddScoped<IComplaintDepartmentFeedbackQueryRepository, ComplaintDepartmentFeedbackQueryRepository>();

            // ── Complaint Resolution Repositories ───────────────────────────────
            services.AddScoped<IComplaintResolutionCommandRepository, ComplaintResolutionCommandRepository>();
            services.AddScoped<IComplaintResolutionQueryRepository, ComplaintResolutionQueryRepository>();

            // ── Sales Return Repositories ──────────────────────────────────────
            services.AddScoped<ISalesReturnCommandRepository, SalesReturnCommandRepository>();
            services.AddScoped<ISalesReturnQueryRepository, SalesReturnQueryRepository>();

            // ── Agent Portal Repository ────────────────────────────────────────
            services.AddScoped<IAgentPortalQueryRepository, AgentPortalQueryRepository>();

            // ── Discount Master ────────────────────────────────────────────────
            services.AddScoped<IDiscountMasterCommandRepository, DiscountMasterCommandRepository>();
            services.AddScoped<IDiscountMasterQueryRepository, DiscountMasterQueryRepository>();

            // ── Commission Split ──────────────────────────────────────────────
            services.AddScoped<ICommissionSplitCommandRepository, CommissionSplitCommandRepository>();
            services.AddScoped<ICommissionSplitQueryRepository, CommissionSplitQueryRepository>();

            // ── Trip Sheet Repositories ─────────────────────────────────────────
            services.AddScoped<ITripSheetCommandRepository, TripSheetCommandRepository>();
            services.AddScoped<ITripSheetQueryRepository, TripSheetQueryRepository>();

            // ── Stock Ledger Report Repository ───────────────────────────────────
            services.AddScoped<IStockLedgerReportRepository, StockLedgerReportRepository>();

            // ── Lead Conversion Funnel Report Repository ─────────────────────────
            services.AddScoped<ILeadConversionFunnelRepository, LeadConversionFunnelRepository>();

            // ── Sales Stock Ledger Service (cross-module, non-cached — transactional table) ──
            services.AddScoped<ISalesStockLedgerService, SalesStockLedgerLookupRepository>();

            // ── Sales MiscMaster Lookup (cross-module) ──────────────────────────
            services.AddScoped<ISalesMiscMasterLookup, SalesMiscMasterLookupRepository>();

            // ── Commission Split Lookup (cross-module) ──────────────────────────
            services.AddScoped<ICommissionSplitLookup, CommissionSplitLookupRepository>();

            // ── Sales Invoice Lookup (cross-module, for Finance EInvoice creation) ──
            services.AddScoped<ISalesInvoiceLookup, SalesInvoiceLookupRepository>();

            // ── Agent Customer Mapping Lookup (cross-module, Party → Sales.AgentCustomerMapping) ──
            services.AddScoped<IAgentCustomerMappingLookup, AgentCustomerMappingLookupRepository>();

            // ═══════════════════════════════════════════════════════════════
            // OUTBOX PATTERN SERVICES (SQL-based for transaction atomicity)
            // ═══════════════════════════════════════════════════════════════
            services.AddScoped<IOutboxRepository, OutboxRepository>();
            services.AddScoped<IOutboxEventPublisher, OutboxEventPublisher>();

            // ── GatePass Document Processors (Strategy pattern, write-side) ──
            services.AddScoped<Contracts.Interfaces.IGatePassDocumentProcessor, Repositories.GatePass.InvoiceGatePassProcessor>();
            services.AddScoped<Contracts.Interfaces.IGatePassDocumentProcessor, Repositories.GatePass.DeliveryChallanGatePassProcessor>();

            // ── GatePass Document Resolvers (Strategy pattern, read-side) ───
            services.AddScoped<Contracts.Interfaces.Gate.IGatePassDocResolver, Repositories.GatePass.InvoiceGatePassDocResolver>();
            services.AddScoped<Contracts.Interfaces.Gate.IGatePassDocResolver, Repositories.GatePass.DeliveryChallanGatePassDocResolver>();
            

            // Validation repositories — cross-module referential integrity (Rule 25)
            services.AddScoped<Contracts.Interfaces.Validations.SalesManagement.ISalesCurrencyValidation, Repositories.Validations.SalesCurrencyValidationRepository>();
            services.AddScoped<Contracts.Interfaces.Validations.SalesManagement.IPartyMasterSalesValidation, Repositories.Validations.PartyMasterSalesValidationRepository>();
            services.AddScoped<Contracts.Interfaces.Validations.SalesManagement.ISalesItemValidation, Repositories.Validations.SalesItemValidationRepository>();
            services.AddScoped<Contracts.Interfaces.Validations.SalesManagement.ISalesUomValidation, Repositories.Validations.SalesUomValidationRepository>();
            services.AddScoped<Contracts.Interfaces.Validations.SalesManagement.ISalesHsnValidation, Repositories.Validations.SalesHsnValidationRepository>();
            services.AddScoped<Contracts.Interfaces.Validations.SalesManagement.ILotMasterSalesValidation, Repositories.Validations.LotMasterSalesValidationRepository>();
            services.AddScoped<Contracts.Interfaces.Validations.SalesManagement.IPackTypeSalesValidation, Repositories.Validations.PackTypeSalesValidationRepository>();

            return services;
        }
    }
}