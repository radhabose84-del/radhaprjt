using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FluentValidation;
using MediatR;

using FAM.Infrastructure;
using FAM.API.Validation.Locations;
using FAM.API.Validation.Common;

using FAM.Application.Common.Mappings;
using FAM.Application.Common.Mappings.AssetMaster;
using FAM.Application.Common.Mappings.AssetPurchase;

namespace FixedAssetManagement.Module
{
    public static class ModuleExtensions
    {
        public static IServiceCollection AddFixedAssetManagementModule(
            this IServiceCollection services,
            IConfiguration configuration,
            IHostEnvironment env)
        {
            // 1) MediatR handlers
            // services.AddMediatR(cfg =>
            // {
            //     cfg.RegisterServicesFromAssembly(
            //         typeof(FAM.Application.Location.Command.CreateLocation.CreateLocationCommandHandler).Assembly);
            // });

            // 2) Validators
            services.AddValidatorsFromAssembly(typeof(CreateLocationCommandValidator).Assembly);

            // 3) Module-specific validation infrastructure
            services.AddScoped<FAM.API.Validation.Common.MaxLengthProvider>();
            services.AddScoped<FAM.API.Validation.Common.IMaxLengthProvider>(
                sp => sp.GetRequiredService<FAM.API.Validation.Common.MaxLengthProvider>());
            
            // ❌ REMOVED: IValidationRuleProvider is registered globally in Program.cs

            // 4) Infrastructure
            services.AddFAMInfrastructure(configuration, env);

            // 5) AutoMapper profiles
            services.AddAutoMapper(
                typeof(LocationProfile),
                typeof(AssetGroupProfile),
                typeof(SubLocationProfile),
                typeof(MiscTypeMasterProfile),
                typeof(MiscMasterProfile),
                typeof(DepreciationGroupProfile),
                typeof(AssetCategoriesProfile),
                typeof(AssetSubCategoriesProfile),
                typeof(ManufactureProfile),
                typeof(UOMProfile),
                typeof(AssetMasterGeneralProfile),
                typeof(SpecificationMasterProfile),
                typeof(AssetPurchaseProfile),
                typeof(AssetSpecificationProfile),
                typeof(AssetWarrantyProfile),
                typeof(AssetLocationProfile),
                typeof(AssetAdditionalCostProfile),
                typeof(AssetInsuranceProfile),
                typeof(AssetTransferProfile),
                typeof(AssetAmcProfile),
                typeof(AssetDisposalProfile),
                typeof(DepreciationDetailProfile),
                typeof(AssetIssueTransferApproval),
                typeof(AssetTransferReceiptProfile),
                typeof(AssetAuditProfile),
                typeof(AssetSubGroupProfile),
                typeof(WDVDepreciationDetailProfile)
            );

            return services;
        }
    }
}