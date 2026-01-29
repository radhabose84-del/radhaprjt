using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using Serilog;

using FAM.Application.Common.Interfaces;
using FAM.Application.Common.Interfaces.AuditLog;
using FAM.Application.Common.Interfaces.IAssetCategories;
using FAM.Application.Common.Interfaces.IAssetGroup;
using FAM.Application.Common.Interfaces.IAssetSubCategories;
using FAM.Application.Common.Interfaces.IDashboard;
using FAM.Application.Common.Interfaces.IDepreciationDetail;
using FAM.Application.Common.Interfaces.IDepreciationGroup;
using FAM.Application.Common.Interfaces.IExcelImport;
using FAM.Application.Common.Interfaces.ILocation;
using FAM.Application.Common.Interfaces.IManufacture;
using FAM.Application.Common.Interfaces.IMiscMaster;
using FAM.Application.Common.Interfaces.IMiscTypeMaster;
using FAM.Application.Common.Interfaces.IReports;
using FAM.Application.Common.Interfaces.ISpecificationMaster;
using FAM.Application.Common.Interfaces.ISubLocation;
using FAM.Application.Common.Interfaces.IUOM;
using FAM.Application.Common.Interfaces.IWdvDepreciation;
using FAM.Application.Common.Interfaces.IAssetSubGroup;

using FAM.Application.Common.Interfaces.IAssetMaster.IAssetAdditionalCost;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetAmc;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetInsurance;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetLocation;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetMasterGeneral;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetPurchase;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetSpecification;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetWarranty;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetDisposal;
using FAM.Application.Common.Interfaces.IAssetTransferIssueApproval;
using FAM.Application.Common.Interfaces.IAssetTransferReceipt;

using FAM.Infrastructure.Data;
using FAM.Infrastructure.Helpers;
using FAM.Infrastructure.Repositories;
using FAM.Infrastructure.Repositories.AssetCategories;
using FAM.Infrastructure.Repositories.AssetGroup;
using FAM.Infrastructure.Repositories.AssetSubCategories;
using FAM.Infrastructure.Repositories.AssetSubGroup;
using FAM.Infrastructure.Repositories.Dashboard;
using FAM.Infrastructure.Repositories.DepreciationDetail;
using FAM.Infrastructure.Repositories.DepreciationGroup;
using FAM.Infrastructure.Repositories.ExcelImport;
using FAM.Infrastructure.Repositories.Locations;
using FAM.Infrastructure.Repositories.Manufacture;
using FAM.Infrastructure.Repositories.MiscMaster;
using FAM.Infrastructure.Repositories.MiscTypeMaster;
using FAM.Infrastructure.Repositories.Reports;
using FAM.Infrastructure.Repositories.SpecificationMaster;
using FAM.Infrastructure.Repositories.SubLocation;
using FAM.Infrastructure.Repositories.UOMs;
using FAM.Infrastructure.Repositories.WDVDepreciation;

using FAM.Infrastructure.Repositories.AssetMaster.AssetAdditionalCost;
using FAM.Infrastructure.Repositories.AssetMaster.AssetAmc;
using FAM.Infrastructure.Repositories.AssetMaster.AssetDisposal;
using FAM.Infrastructure.Repositories.AssetMaster.AssetInsurance;
using FAM.Infrastructure.Repositories.AssetMaster.AssetLocation;
using FAM.Infrastructure.Repositories.AssetMaster.AssetMasterGeneral;
using FAM.Infrastructure.Repositories.AssetMaster.AssetPurchase;
using FAM.Infrastructure.Repositories.AssetMaster.AssetSpecification;
using FAM.Infrastructure.Repositories.AssetMaster.AssetTransfer;
using FAM.Infrastructure.Repositories.AssetMaster.AssetTransferIssue;
using FAM.Infrastructure.Repositories.AssetMaster.AssetWarranty;
using FAM.Infrastructure.Repositories.AssetTransferIssueApproval;
using FAM.Infrastructure.Repositories.AssetTransferReceipt;

using FAM.Infrastructure.Services;
using Infrastructure.Data;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetTransferIssue; // MongoDbContext

namespace FAM.Infrastructure
{
    public static class DependencyInjection
    {
        // ✅ This is the method Program.cs / Module should call
        public static IServiceCollection AddFAMInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration,
            IHostEnvironment env)
        {
            // --------------------------
            // SQL Connection
            // --------------------------
            var raw = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(raw))
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found or is empty.");

            var connectionString = raw
                .Replace("{SERVER}", Environment.GetEnvironmentVariable("DATABASE_SERVER") ?? "")
                .Replace("{USER_ID}", Environment.GetEnvironmentVariable("DATABASE_USERID") ?? "")
                .Replace("{ENC_PASSWORD}", Environment.GetEnvironmentVariable("DATABASE_PASSWORD") ?? "");

            // DbContext (skip in Testing if you want in-memory later)
            if (!env.IsEnvironment("Testing"))
            {
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(connectionString, sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                    }));
            }

            // Dapper
            services.AddTransient<IDbConnection>(_ => new SqlConnection(connectionString));

            // --------------------------
            // MongoDB (AuditLog + Outbox usage)
            // --------------------------
            var mongoConn = configuration.GetConnectionString("MongoDbConnectionString");
            if (string.IsNullOrWhiteSpace(mongoConn))
                throw new InvalidOperationException("MongoDbConnectionString is missing or empty.");

            var mongoDbName = configuration["MongoDb:DatabaseName"];
            if (string.IsNullOrWhiteSpace(mongoDbName))
                throw new InvalidOperationException("MongoDb:DatabaseName is missing or empty.");

            services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoConn));

            services.AddSingleton<IMongoDbContext>(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                return new MongoDbContext(client, mongoDbName);
            });

            // Optional: IMongoDatabase if anything uses it directly
            services.AddSingleton(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                return client.GetDatabase(mongoDbName);
            });

            // --------------------------
            // Logging
            // --------------------------
            services.AddLogging(b => b.AddSerilog());

            // --------------------------
            // Common Services
            // --------------------------
            services.AddHttpContextAccessor();
            services.AddTransient<AuthTokenHandler>();
            services.AddScoped<IIPAddressService, IPAddressService>();
            services.AddTransient<IFileUploadService, FileUploadRepository>();
            services.AddSingleton<ITimeZoneService, TimeZoneService>();
            services.AddTransient<IJwtTokenHelper, JwtTokenHelper>();

            // --------------------------
            // Repositories
            // --------------------------
            services.AddScoped<IAuditLogRepository, AuditLogRepository>();

            services.AddScoped<IAssetGroupCommandRepository, AssetGroupCommandRepository>();
            services.AddScoped<IAssetGroupQueryRepository, AssetGroupQueryRepository>();

            services.AddScoped<ILocationCommandRepository, LocationCommandRepository>();
            services.AddScoped<ILocationQueryRepository, LocationQueryRepository>();

            services.AddScoped<ISubLocationCommandRepository, SubLocationCommandRepository>();
            services.AddScoped<ISubLocationQueryRepository, SubLocationQueryRepository>();

            services.AddScoped<IMiscTypeMasterQueryRepository, MiscTypeMasterQueryRepository>();
            services.AddScoped<IMiscTypeMasterCommandRepository, MiscTypeMasterCommandRepository>();

            services.AddScoped<IMiscMasterQueryRepository, MiscMasterQueryRepository>();
            services.AddScoped<IMiscMasterCommandRepository, MiscMasterCommandRepository>();

            services.AddScoped<IDepreciationGroupCommandRepository, DepreciationGroupCommandRepository>();
            services.AddScoped<IDepreciationGroupQueryRepository, DepreciationGroupQueryRepository>();

            services.AddScoped<IAssetCategoriesQueryRepository, AssetCategoriesQueryRepository>();
            services.AddScoped<IAssetCategoriesCommandRepository, AssetCategoriesCommandRepository>();

            services.AddScoped<IAssetSubCategoriesQueryRepository, AssetSubCategoriesQueryRepository>();
            services.AddScoped<IAssetSubCategoriesCommandRepository, AssetSubCategoriesCommandRepository>();

            services.AddScoped<IManufactureCommandRepository, ManufactureCommandRepository>();
            services.AddScoped<IManufactureQueryRepository, ManufactureQueryRepository>();

            services.AddScoped<IUOMCommandRepository, UOMCommandRepository>();
            services.AddScoped<IUOMQueryRepository, UOMQueryRepository>();

            services.AddScoped<IAssetMasterGeneralCommandRepository, AssetMasterGeneralCommandRepository>();
            services.AddScoped<IAssetMasterGeneralQueryRepository, AssetMasterGeneralQueryRepository>();

            services.AddScoped<IAssetPurchaseQueryRepository, AssetPurchaseQueryRepository>();
            services.AddScoped<IAssetPurchaseCommandRepository, AssetPurchaseCommandRepository>();

            services.AddScoped<ISpecificationMasterCommandRepository, SpecificationMasterCommandRepository>();
            services.AddScoped<ISpecificationMasterQueryRepository, SpecificationMasterQueryRepository>();

            services.AddScoped<IAssetSpecificationCommandRepository, AssetSpecificationCommandRepository>();
            services.AddScoped<IAssetSpecificationQueryRepository, AssetSpecificationQueryRepository>();

            services.AddScoped<IAssetLocationQueryRepository, AssetLocationQueryRepository>();
            services.AddScoped<IAssetLocationCommandRepository, AssetLocationCommandRepository>();

            services.AddScoped<IAssetAdditionalCostQueryRepository, AssetAdditionalCostQueryRepository>();
            services.AddScoped<IAssetAdditionalCostCommandRepository, AssetAdditionalCostCommandRepository>();

            services.AddScoped<IAssetWarrantyQueryRepository, AssetWarrantyQueryRepository>();
            services.AddScoped<IAssetWarrantyCommandRepository, AssetWarrantyCommandRepository>();

            services.AddScoped<IAssetInsuranceCommandRepository, AssetInsuranceCommandRepository>();
            services.AddScoped<IAssetInsuranceQueryRepository, AssetInsuranceQueryRepository>();

            services.AddScoped<IAssetAmcQueryRepository, AssetAmcQueryRepository>();
            services.AddScoped<IAssetAmcCommandRepository, AssetAmcCommandRepository>();

            services.AddScoped<IAssetTransferQueryRepository, AssetTransferQueryRepository>();
            services.AddScoped<IAssetTransferCommandRepository, AssetTransferCommandRepository>();

            services.AddScoped<IAssetDisposalQueryRepository, AssetDisposalQueryRepository>();
            services.AddScoped<IAssetDisposalCommandRepository, AssetDisposalCommandRepository>();

            services.AddScoped<IDepreciationDetailCommandRepository, DepreciationDetailCommandRepository>();
            services.AddScoped<IDepreciationDetailQueryRepository, DepreciationDetailQueryRepository>();

            services.AddScoped<IAssetTransferIssueApprovalQueryRepository, AssetTransferIssueQueryRepository>();
            services.AddScoped<IAssetTransferIssueApprovalCommandRepository, AssetTransferIssueCommandRepository>();

            services.AddScoped<IAssetTransferReceiptQueryRepository, AssetTransferReceiptQueryRepository>();
            services.AddScoped<IAssetTransferReceiptCommandRepository, AssetTransferReceiptCommandRepository>();

            services.AddScoped<IExcelImportCommandRepository, ExcelImportCommandRepository>();
            services.AddScoped<IExcelImportQueryRepository, ExcelImportCommandQueryRepository>();

            services.AddScoped<IReportRepository, ReportsRepository>();

            services.AddScoped<IAssetSubGroupCommandRepository, AssetSubGroupCommandRepository>();
            services.AddScoped<IAssetSubGroupQueryRepository, AssetSubGroupQueryRepository>();

            services.AddScoped<IWdvDepreciationQueryRepository, WdvDepreciationQueryRepository>();
            services.AddScoped<IWdvDepreciationCommandRepository, WdvDepreciationCommandRepository>();

            services.AddScoped<IDashboardQueryRepository, DashboardQueryRepository>();

            // ✅ IMPORTANT: Do NOT register AutoMapper here if you want ONE place.
            // Module will register AutoMapper profiles.

            return services;
        }
    }
}
