using FluentValidation;
using FAM.Presentation.Validation.Locations;
using FAM.Application.Location.Command.CreateLocation;
using FAM.Application.Location.Command.UpdateLocation;
using FAM.Presentation.Validation.SubLocation;
using FAM.Application.Location.Command.UpdateSubLocation;
using FAM.Application.SubLocation.Command.CreateSubLocation;
using FAM.Presentation.Validation.MiscTypeMaster;
using FAM.Application.MiscTypeMaster.Command.CreateMiscTypeMaster;
using FAM.Application.MiscTypeMaster.Command.UpdateMiscTypeMaster;
using FAM.Presentation.Validation.DepreciationGroup;
using FAM.Application.DepreciationGroup.Commands.UpdateDepreciationGroup;
using FAM.Application.DepreciationGroup.Commands.CreateDepreciationGroup;
using FAM.Application.AssetGroup.Command.CreateAssetGroup;
using FAM.Presentation.Validation.AssetGroup;
using FAM.Application.AssetGroup.Command.UpdateAssetGroup;
using FAM.Presentation.Validation.AssetCategories;
using FAM.Application.AssetCategories.Command.CreateAssetCategories;
using FAM.Application.AssetCategories.Command.UpdateAssetCategories;
using FAM.Application.AssetSubCategories.Command.CreateAssetSubCategories;
using FAM.Application.AssetSubCategories.Command.UpdateAssetSubCategories;
using FAM.Presentation.Validation.AssetSubCategories;
using FAM.Application.MiscMaster.Command.CreateMiscMaster;
using FAM.Presentation.Validation.MiscMaster;
using FAM.Application.MiscMaster.Command.UpdateMiscMaster;
using FAM.Application.Manufacture.Commands.UpdateManufacture;
using FAM.Presentation.Validation.Manufacture;
using FAM.Application.Manufacture.Commands.CreateManufacture;
using FAM.Presentation.Validation.AssetMaster.AssetMasterGeneral;
using FAM.Application.AssetMaster.AssetMasterGeneral.Commands.CreateAssetMasterGeneral;
using FAM.Application.AssetMaster.AssetMasterGeneral.Commands.UpdateAssetMasterGeneral;
using FAM.Application.AssetMaster.AssetMasterGeneral.Commands.UploadAssetMasterGeneral;
using FAM.Application.UOM.Command.CreateUOM;
using FAM.Application.UOM.Command.UpdateUOM;
using FAM.Presentation.Validation.UOM;
using FAM.Application.SpecificationMaster.Commands.UpdateSpecificationMaster;
using FAM.Presentation.Validation.SpecificationMaster;
using FAM.Application.SpecificationMaster.Commands.CreateSpecificationMaster;
using FAM.Application.AssetLocation.Commands.CreateAssetLocation;
using FAM.Presentation.Validation.AssetMaster.AssetLocation;
using FAM.Application.AssetMaster.AssetLocation.Commands.UpdateAssetLocation;
using FAM.Application.AssetMaster.AssetPurchase.Commands.CreateAssetPurchaseDetails;
using FAM.Presentation.Validation.AssetMaster.AssetPurchase;
using FAM.Application.AssetMaster.AssetPurchase.Commands.UpdateAssetPurchaseDetails;
using FAM.Application.AssetMaster.AssetSpecification.Commands.CreateAssetSpecification;
using FAM.Application.AssetMaster.AssetSpecification.Commands.UpdateAssetSpecification;
using FAM.Presentation.Validation.AssetMaster.AssetSpecification;
using FAM.Application.AssetMaster.AssetAdditionalCost.Commands.CreateAssetAdditionalCost;
using FAM.Presentation.Validation.AssetMaster.AssetAdditionalCost;
using FAM.Application.AssetMaster.AssetAdditionalCost.Commands.UpdateAssetAdditionalCost;
using FAM.Application.AssetMaster.AssetWarranty.Commands.CreateAssetWarranty;
using FAM.Presentation.Validation.AssetMaster.AssetWarranty;
using FAM.Application.AssetMaster.AssetWarranty.Commands.UpdateAssetWarranty;
using FAM.Presentation.Validation.AssetMaster.AssetWaranty;
using FAM.Application.AssetMaster.AssetWarranty.Commands.UploadAssetWarranty;
using FAM.Application.AssetMaster.AssetInsurance.Commands.CreateAssetInsurance;
using FAM.Application.AssetMaster.AssetInsurance.Commands.UpdateAssetInsurance;
using FAM.Presentation.Validation.AssetMaster.AssetInsurance;
using FAM.Application.AssetMaster.AssetDisposal.Command.CreateAssetDisposal;
using FAM.Presentation.Validation.AssetMaster.AssetDisposal;
using FAM.Application.AssetMaster.AssetDisposal.Command.UpdateAssetDisposal;
using FAM.Application.AssetMaster.AssetTranferIssueApproval.Commands.UpdateAssetTranferIssueApproval;
using FAM.Application.AssetMaster.AssetTransferIssue.Command.CreateAssetTransferIssue;
using FAM.Presentation.Validation.AssetMaster.AssetTransferIssueApproval;
using FAM.Presentation.Validation.AssetMaster.AssetTransferIssue;
using FAM.Application.AssetMaster.AssetTransferIssue.Command.UpdateAssetTransferIssue;
using FAM.Presentation.Validation.AssetMaster.AssetTransferReceipt;
using FAM.Application.AssetMaster.AssetTransferReceipt.Command.CreateAssetTransferReceipt;
using FAM.Application.AssetMaster.AssetMasterGeneral.Commands.DeleteAssetMasterGeneral;
using FAM.Application.DepreciationGroup.Commands.DeleteDepreciationGroup;
using FAM.Application.AssetMaster.AssetWarranty.Commands.DeleteAssetWarranty;
using FAM.Application.AssetMaster.AssetSpecification.Commands.DeleteAssetSpecification;
using FAM.Application.SpecificationMaster.Commands.DeleteSpecificationMaster;
using FAM.Application.AssetMaster.AssetAmc.Command.CreateAssetAmc;
using FAM.Application.AssetMaster.AssetAmc.Command.UpdateAssetAmc;
using FAM.Presentation.Validation.AssetMaster.AssetAmc;
using FAM.Application.AssetMaster.AssetMasterGeneral.Commands.UploadDocumentAssetMaster;
using FAM.Application.AssetSubGroup.Command.CreateAssetSubGroup;
using FAM.Presentation.Validation.AssetSubGroup;
using FAM.Application.AssetSubGroup.Command.UpdateAssetSubGroup;
using Microsoft.Extensions.DependencyInjection;


namespace FAM.Presentation.Validation.Common
{
    public class ValidationService
    {
        public void AddValidationServices(IServiceCollection services)
        {
            services.AddScoped<MaxLengthProvider>();
            services.AddScoped<IValidator<CreateAssetGroupCommand>, CreateAssetGroupCommandValidator>();
            services.AddScoped<IValidator<UpdateAssetGroupCommand>, UpdateAssetGroupCommandValidator>();
            services.AddScoped<IValidator<CreateAssetCategoriesCommand>, CreateAssetCategoriesCommandValidator>();
            services.AddScoped<IValidator<UpdateAssetCategoriesCommand>, UpdateAssetCategoriesCommandValidator>();
            services.AddScoped<IValidator<CreateLocationCommand>, CreateLocationCommandValidator>();
            services.AddScoped<IValidator<UpdateLocationCommand>, UpdateLocationCommandValidator>();
            services.AddScoped<IValidator<CreateSubLocationCommand>, CreateSubLocationCommandValidator>();
            services.AddScoped<IValidator<UpdateSubLocationCommand>, UpdateSubLocationCommandValidator>();
            services.AddScoped<IValidator<CreateMiscTypeMasterCommand>, CreateMiscTypeMasterCommandValidator>();
            services.AddScoped<IValidator<UpdateMiscTypeMasterCommand>, UpdateMiscTypeMasterCommandValidator>();
            services.AddScoped<IValidator<CreateDepreciationGroupCommand>, CreateDepreciationGroupCommandValidator>();
            services.AddScoped<IValidator<CreateMiscTypeMasterCommand>, CreateMiscTypeMasterCommandValidator>();
            services.AddScoped<IValidator<UpdateMiscTypeMasterCommand>, UpdateMiscTypeMasterCommandValidator>();            
            services.AddScoped<IValidator<UpdateDepreciationGroupCommand>, UpdateDepreciationGroupCommandValidator>();
            services.AddScoped<IValidator<CreateAssetSubCategoriesCommand>, CreateAssetSubCategoriesCommandValidator>();
            services.AddScoped<IValidator<UpdateAssetSubCategoriesCommand>, UpdateAssetSubCategoriesCommandValidator>();
            services.AddScoped<IValidator<CreateMiscMasterCommand>, CreateMiscMasterCommandValidator>();
            services.AddScoped<IValidator<UpdateMiscMasterCommand>, UpdateMiscMasterCommandValidator>();
            services.AddScoped<IValidator<UpdateManufactureCommand>, UpdateManufactureCommandValidator>();
            services.AddScoped<IValidator<UpdateManufactureCommand>, UpdateManufactureCommandValidator>();
            services.AddScoped<IValidator<CreateManufactureCommand>, CreateManufactureCommandValidator>();
            services.AddScoped<IValidator<CreateAssetMasterGeneralCommand>, CreateAssetMasterGeneralCommandValidator>();
            services.AddScoped<IValidator<UpdateAssetMasterGeneralCommand>, UpdateAssetMasterGeneralCommandValidator>();
            services.AddScoped<IValidator<DeleteAssetMasterGeneralCommand>, DeleteAssetMasterGeneralCommandValidator>();
            services.AddScoped<IValidator<UploadFileAssetMasterGeneralCommand>, UploadAssetMasterGeneralCommandValidator>();
            services.AddScoped<IValidator<CreateUOMCommand>, CreateUOMCommandValidator>();
            services.AddScoped<IValidator<UpdateUOMCommand>, UpdateUOMCommandValidator>();
            services.AddScoped<IValidator<UpdateSpecificationMasterCommand>, UpdateSpecificationMasterCommandValidator>();
            services.AddScoped<IValidator<CreateSpecificationMasterCommand>, CreateSpecificationMasterCommandValidator>();
            services.AddScoped<IValidator<CreateAssetLocationCommand>, CreateAssetLocationCommandValidator>();
            services.AddScoped<IValidator<UpdateAssetLocationCommand>, UpdateAssetLocationCommandValidator>();
            services.AddScoped<IValidator<CreateAssetPurchaseDetailCommand>, CreateAssetPurchaseDetailCommandValidator>();
            services.AddScoped<IValidator<UpdateAssetPurchaseDetailCommand>, UpdateAssetPurchaseDetailCommandValidator>();
            services.AddScoped<IValidator<CreateAssetSpecificationCommand>, CreateAssetSpecificationCommandValidator>();
            services.AddScoped<IValidator<UpdateAssetSpecificationCommand>, UpdateAssetSpecificationCommandValidator>();
            services.AddScoped<IValidator<CreateAssetAdditionalCostCommand>, CreateAssetAdditionalCostCommandValidator>();
            services.AddScoped<IValidator<UpdateAssetAdditionalCostCommand>, UpdateAssetAdditionalCostCommandValidator>();
            services.AddScoped<IValidator<CreateAssetWarrantyCommand>, CreateAssetWarrantyCommandValidator>();
            services.AddScoped<IValidator<UpdateAssetWarrantyCommand>, UpdateAssetWarrantyCommandValidator>();
            services.AddScoped<IValidator<UploadFileAssetWarrantyCommand>, UploadAssetWarrantyCommandValidator>();
            services.AddScoped<IValidator<CreateAssetInsuranceCommand>, CreateAssetInsuranceCommandValidator>();
            services.AddScoped<IValidator<UpdateAssetInsuranceCommand>, UpdateAssetInsuranceCommandValidator>();
            services.AddScoped<IValidator<CreateAssetDisposalCommand>, CreateAssetDisposalCommandValidator>();
            services.AddScoped<IValidator<DeleteDepreciationGroupCommand>, DeleteDepreciationGroupCommandValidator>();
            services.AddScoped<IValidator<DeleteAssetWarrantyCommand>, DeleteAssetWarrantyCommandValidator>();
            services.AddScoped<IValidator<DeleteAssetSpecificationCommand>, DeleteAssetSpecificationCommandValidator>();
            services.AddScoped<IValidator<DeleteSpecificationMasterCommand>, DeleteSpecificationMasterCommandValidator>();




            services.AddScoped<IValidator<UpdateAssetDisposalCommand>, UpdateAssetDisposalCommandValidator>();
            services.AddScoped<IValidator<UpdateAssetTranferIssueApprovalCommand>, UpdateAssetTransferIssueApprovalValidator>();
            services.AddScoped<IValidator<CreateAssetTransferIssueCommand>, CreateAssetTransferIssueCommandValidator>();
            services.AddScoped<IValidator<UpdateAssetTransferIssueCommand>, UpdateAssetTransferIssueCommandValidator>();
            services.AddScoped<IValidator<CreateAssetTransferReceiptCommand>, CreateAssetTransferReceiptCommandValidator>();
            services.AddScoped<IValidator<CreateAssetAmcCommand>, CreateAssetAmcCommandValidator>();
            services.AddScoped<IValidator<UpdateAssetAmcCommand>, UpdateAssetAmcCommandValidator>();

            services.AddScoped<IValidator<UploadDocumentAssetMasterGeneralCommand>, UploadDocumentAssetMasterGeneralCommandValidator>();
            services.AddScoped<IValidator<CreateAssetSubGroupCommand>, CreateAssetSubGroupCommandValidator>();
            services.AddScoped<IValidator<UpdateAssetSubGroupCommand>, UpdateAssetSubGroupCommandValidator>();


    }  
    }
}