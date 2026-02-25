using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FluentValidation;

using FAM.Infrastructure;
using FAM.Presentation.Validation.Locations;
using FAM.Presentation.Validation.Common;

using FAM.Application.Common.Mappings;

namespace FixedAssetManagement.Module
{
    public static class ModuleExtensions
    {
        public static IServiceCollection AddFixedAssetManagementModule(
            this IServiceCollection services,
            IConfiguration configuration,
            IHostEnvironment env)
        {
            // ✅ 0) Infrastructure FIRST (DbContext + repos + services)
            services.AddFAMInfrastructure(configuration, env);

            // ✅ 1) Use compile-time assemblies (NO Assembly.Load)
            var applicationAssembly = typeof(LocationProfile).Assembly;              // FAM.Application
            var apiAssembly = typeof(CreateLocationCommandValidator).Assembly;       // FAM.Presentation

            // ✅ 2) MediatR handlers from Application (register ALL handlers)
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));

            // ✅ 3) AutoMapper profiles from Application (register ALL profiles)
            services.AddAutoMapper(applicationAssembly);

            // ✅ 4) Validators from API (register ALL validators)
            services.AddValidatorsFromAssembly(apiAssembly);

            // ✅ 5) Module-specific validation infrastructure (used by validators)
            services.AddScoped<MaxLengthProvider>();
            services.AddScoped<IMaxLengthProvider>(sp => sp.GetRequiredService<MaxLengthProvider>());

            // ❌ NOTE: IValidationRuleProvider is registered globally in Program.cs (keep it there)

            // 1) MediatR handlers
            // services.AddMediatR(cfg =>
            // {
            //     cfg.RegisterServicesFromAssembly(
            //         typeof(FAM.Application.Location.Command.CreateLocation.CreateLocationCommandHandler).Assembly);
            // });

            // 2) Validators
            // services.AddValidatorsFromAssembly(typeof(CreateLocationCommandValidator).Assembly);

            // 3) Module-specific validation infrastructure
            // services.AddScoped<FAM.Presentation.Validation.Common.MaxLengthProvider>();
            // services.AddScoped<FAM.Presentation.Validation.Common.IMaxLengthProvider>(
            //     sp => sp.GetRequiredService<FAM.Presentation.Validation.Common.MaxLengthProvider>());


            // 4) Infrastructure
            // // services.AddFAMInfrastructure(configuration, env);

            // // ✅ 2) Use compile-time assemblies (NO Assembly.Load)
            // var applicationAssembly = typeof(LocationProfile).Assembly;                 // MaintenanceManagement.Application
            // var apiAssembly = typeof(CreateLocationCommandValidator).Assembly;          // MaintenanceManagement.Presentation

            // // ✅ 3) MediatR handlers from Application
            // services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));

            // // 5) AutoMapper profiles
            // services.AddAutoMapper(
            //     typeof(LocationProfile),
            //     typeof(AssetGroupProfile),
            //     typeof(SubLocationProfile),
            //     typeof(MiscTypeMasterProfile),
            //     typeof(MiscMasterProfile),
            //     typeof(DepreciationGroupProfile),
            //     typeof(AssetCategoriesProfile),
            //     typeof(AssetSubCategoriesProfile),
            //     typeof(ManufactureProfile),
            //     typeof(UOMProfile),
            //     typeof(AssetMasterGeneralProfile),
            //     typeof(SpecificationMasterProfile),
            //     typeof(AssetPurchaseProfile),
            //     typeof(AssetSpecificationProfile),
            //     typeof(AssetWarrantyProfile),
            //     typeof(AssetLocationProfile),
            //     typeof(AssetAdditionalCostProfile),
            //     typeof(AssetInsuranceProfile),
            //     typeof(AssetTransferProfile),
            //     typeof(AssetAmcProfile),
            //     typeof(AssetDisposalProfile),
            //     typeof(DepreciationDetailProfile),
            //     typeof(AssetIssueTransferApproval),
            //     typeof(AssetTransferReceiptProfile),
            //     typeof(AssetAuditProfile),
            //     typeof(AssetSubGroupProfile),
            //     typeof(WDVDepreciationDetailProfile)
            // );

            return services;
        }
    }
}