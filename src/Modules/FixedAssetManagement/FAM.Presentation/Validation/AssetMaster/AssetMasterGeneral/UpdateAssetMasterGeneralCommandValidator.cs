using FAM.Application.AssetMaster.AssetLocation.Commands.UpdateAssetLocation;
using FAM.Application.AssetMaster.AssetMasterGeneral.Commands.UpdateAssetMasterGeneral;
using FAM.Application.AssetMaster.AssetPurchase.Commands.UpdateAssetPurchaseDetails;
using FAM.Domain.Entities;
using FAM.Domain.Entities.AssetPurchase;
using FAM.Presentation.Validation.Common;
using FluentValidation;
using Shared.Validation.Common;

namespace FAM.Presentation.Validation.AssetMaster.AssetMasterGeneral
{
    public class UpdateAssetMasterGeneralCommandValidator : AbstractValidator<UpdateAssetMasterGeneralCommand>
    {
        private readonly List<ValidationRule> _validationRules;

        public UpdateAssetMasterGeneralCommandValidator(MaxLengthProvider maxLengthProvider)
        {
            // Get max lengths dynamically using MaxLengthProvider
            var assetMasterGeneralCodeMaxLength = maxLengthProvider.GetMaxLength<AssetMasterGenerals>("AssetCode")??50;
            var assetMasterGeneralNameMaxLength = maxLengthProvider.GetMaxLength<AssetMasterGenerals>("AssetName")??100;            
            var assetMasterGeneralDescriptionMaxLength = maxLengthProvider.GetMaxLength<AssetMasterGenerals>("AssetDescription")??1000;  
            var assetMasterGeneralMachineCodeMaxLength = maxLengthProvider.GetMaxLength<AssetMasterGenerals>("MachineCode")??100;  

            var BudgetType = maxLengthProvider.GetMaxLength<AssetPurchaseDetails>("BudgetType") ?? 50;
            var OldUnitId = maxLengthProvider.GetMaxLength<AssetPurchaseDetails>("OldUnitId") ?? 2;
            var VendorCode = maxLengthProvider.GetMaxLength<AssetPurchaseDetails>("VendorCode") ?? 50;
            var VendorName = maxLengthProvider.GetMaxLength<AssetPurchaseDetails>("VendorName") ?? 400;
            var PoNo = maxLengthProvider.GetMaxLength<AssetPurchaseDetails>("PoNo") ?? 10;
            var PoSno = maxLengthProvider.GetMaxLength<AssetPurchaseDetails>("PoSno") ?? 4;
            var ItemCode = maxLengthProvider.GetMaxLength<AssetPurchaseDetails>("ItemCode") ?? 100;
            var ItemName = maxLengthProvider.GetMaxLength<AssetPurchaseDetails>("ItemName") ?? 500;
            var GrnNo = maxLengthProvider.GetMaxLength<AssetPurchaseDetails>("GrnNo") ?? 10;
            var GrnSno = maxLengthProvider.GetMaxLength<AssetPurchaseDetails>("GrnSno") ?? 4;
            var BillNo = maxLengthProvider.GetMaxLength<AssetPurchaseDetails>("BillNo") ?? 100;
            var Uom = maxLengthProvider.GetMaxLength<AssetPurchaseDetails>("Uom") ?? 20;
            var BinLocation = maxLengthProvider.GetMaxLength<AssetPurchaseDetails>("BinLocation") ?? 100;
            var PjYear = maxLengthProvider.GetMaxLength<AssetPurchaseDetails>("BinLocation") ?? 8;
            var PjDocId = maxLengthProvider.GetMaxLength<AssetPurchaseDetails>("PjYear") ?? 40;
            var PjDocSr = maxLengthProvider.GetMaxLength<AssetPurchaseDetails>("PjDocSr") ?? 40;
            var PjDocNo = maxLengthProvider.GetMaxLength<AssetPurchaseDetails>("PjDocNo") ?? 10;
            var AssetId = maxLengthProvider.GetMaxLength<AssetPurchaseDetails>("AssetId") ?? 10;
            var AssetSourceId = maxLengthProvider.GetMaxLength<AssetPurchaseDetails>("AssetSourceId") ?? 10;

            // Load validation rules from JSON or another source
            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules is null || !_validationRules.Any())
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }

            // Loop through the rules and apply them
            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.AssetMaster!.AssetName ?? string.Empty)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateAssetMasterGeneralCommand.AssetMaster.AssetName)} {rule.Error}");
                        RuleFor(x => x.AssetMaster!.AssetGroupId)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateAssetMasterGeneralCommand.AssetMaster.AssetGroupId)} {rule.Error}");
                        RuleFor(x => x.AssetMaster!.CompanyId)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateAssetMasterGeneralCommand.AssetMaster.CompanyId)} {rule.Error}");
                        RuleFor(x => x.AssetMaster!.AssetGroupId)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateAssetMasterGeneralCommand.AssetMaster.AssetGroupId)} {rule.Error}");
                        RuleFor(x => x.AssetMaster!.AssetCategoryId)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateAssetMasterGeneralCommand.AssetMaster.AssetCategoryId)} {rule.Error}");
                        RuleFor(x => x.AssetMaster!.AssetSubCategoryId)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateAssetMasterGeneralCommand.AssetMaster.AssetSubCategoryId)} {rule.Error}");
                        RuleFor(x => x.AssetMaster!.AssetType)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateAssetMasterGeneralCommand.AssetMaster.AssetType)} {rule.Error}");
                        RuleFor(x => x.AssetMaster!.Quantity)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateAssetMasterGeneralCommand.AssetMaster.Quantity)} {rule.Error}");
                        RuleFor(x => x.AssetMaster!.UOMId)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateAssetMasterGeneralCommand.AssetMaster.UOMId)} {rule.Error}");
                        RuleFor(x => x.AssetMaster!.WorkingStatus)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateAssetMasterGeneralCommand.AssetMaster.WorkingStatus)} {rule.Error}");
                        //Location
                      RuleFor(x => x.AssetMaster!.AssetLocation!.UnitId)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateAssetLocationCommand.UnitId)} {rule.Error}")
                            .WithMessage($"{nameof(UpdateAssetLocationCommand.UnitId)} must be a valid number.");
                         RuleFor(x => x.AssetMaster!.AssetLocation!.DepartmentId)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateAssetLocationCommand.DepartmentId)} {rule.Error}")
                            .WithMessage($"{nameof(UpdateAssetLocationCommand.DepartmentId)} must be a valid number.");
                        RuleFor(x => x.AssetMaster!.AssetLocation!.LocationId)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateAssetLocationCommand.LocationId)} {rule.Error}")
                            .WithMessage($"{nameof(UpdateAssetLocationCommand.LocationId)} must be a valid number.");
                            RuleFor(x => x.AssetMaster!.AssetLocation!.SubLocationId)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateAssetLocationCommand.SubLocationId)} {rule.Error}")
                            .WithMessage($"{nameof(UpdateAssetLocationCommand.SubLocationId)} must be a valid number.");
                        //Purchase
                        RuleForEach(x => x.AssetMaster!.AssetPurchaseDetails)
                            .ChildRules(purchase =>
                            {
                                purchase.RuleFor(x => x.VendorCode)
                                    .NotEmpty()
                                    .WithMessage($"{nameof(UpdateAssetPurchaseDetailCommand.VendorCode)} {rule.Error}");
                                  purchase.RuleFor(x => x.VendorName)
                                    .NotEmpty()
                                    .WithMessage($"{nameof(UpdateAssetPurchaseDetailCommand.VendorName)} {rule.Error}");
                                  purchase.RuleFor(x => x.PoNo)
                                    .NotEmpty()
                                    .WithMessage($"{nameof(UpdateAssetPurchaseDetailCommand.PoNo)} {rule.Error}");
                                  purchase.RuleFor(x => x.PoSno)
                                    .NotEmpty()
                                    .WithMessage($"{nameof(UpdateAssetPurchaseDetailCommand.PoSno)} {rule.Error}");
                                  purchase.RuleFor(x => x.ItemCode)
                                    .NotEmpty()
                                    .WithMessage($"{nameof(UpdateAssetPurchaseDetailCommand.ItemCode)} {rule.Error}");
                                      purchase.RuleFor(x => x.ItemName)
                                    .NotEmpty()
                                    .WithMessage($"{nameof(UpdateAssetPurchaseDetailCommand.ItemName)} {rule.Error}");
                                  purchase.RuleFor(x => x.GrnNo)
                                    .NotEmpty()
                                    .WithMessage($"{nameof(UpdateAssetPurchaseDetailCommand.GrnNo)} {rule.Error}");
                                  purchase.RuleFor(x => x.GrnSno)
                                    .NotEmpty()
                                    .WithMessage($"{nameof(UpdateAssetPurchaseDetailCommand.GrnSno)} {rule.Error}");
                                  purchase.RuleFor(x => x.AcceptedQty)
                                    .NotEmpty()
                                    .WithMessage($"{nameof(UpdateAssetPurchaseDetailCommand.AcceptedQty)} {rule.Error}");
                                purchase.RuleFor(x => x.PurchaseValue)
                                    .NotEmpty()
                                    .WithMessage($"{nameof(UpdateAssetPurchaseDetailCommand.PurchaseValue)} {rule.Error}");
                                purchase.RuleFor(x => x.GrnValue)
                                    .NotEmpty()
                                    .WithMessage($"{nameof(UpdateAssetPurchaseDetailCommand.GrnValue)} {rule.Error}");
                                purchase.RuleFor(x => x.BillNo)
                                    .NotEmpty()
                                    .WithMessage($"{nameof(UpdateAssetPurchaseDetailCommand.BillNo)} {rule.Error}");
                                purchase.RuleFor(x => x.PjYear)
                                    .NotEmpty()
                                    .WithMessage($"{nameof(UpdateAssetPurchaseDetailCommand.PjYear)} {rule.Error}");
                                purchase.RuleFor(x => x.PjDocId)
                                    .NotEmpty()
                                    .WithMessage($"{nameof(UpdateAssetPurchaseDetailCommand.PjDocId)} {rule.Error}");
                                purchase.RuleFor(x => x.PjDocNo)
                                    .NotEmpty()
                                    .WithMessage($"{nameof(UpdateAssetPurchaseDetailCommand.PjDocNo)} {rule.Error}");
                            });
                        break;
                    case "MaxLength":
                        RuleFor(x => x.AssetMaster!.AssetName)
                            .MaximumLength(assetMasterGeneralNameMaxLength)
                            .WithMessage($"{nameof(UpdateAssetMasterGeneralCommand.AssetMaster.AssetName)} {rule.Error} {assetMasterGeneralNameMaxLength}");
                        RuleFor(x => x.AssetMaster!.AssetDescription)
                            .MaximumLength(assetMasterGeneralDescriptionMaxLength)
                            .WithMessage($"{nameof(UpdateAssetMasterGeneralCommand.AssetMaster.AssetDescription)} {rule.Error} {assetMasterGeneralDescriptionMaxLength}");
                        RuleFor(x => x.AssetMaster!.MachineCode)
                            .MaximumLength(assetMasterGeneralMachineCodeMaxLength)
                            .WithMessage($"{nameof(UpdateAssetMasterGeneralCommand.AssetMaster.MachineCode)} {rule.Error} {assetMasterGeneralMachineCodeMaxLength}");
                        RuleForEach(x => x.AssetMaster!.AssetPurchaseDetails)
                            .ChildRules(purchase =>
                            {
                                purchase.RuleFor(x => x.VendorCode)
                                     .MaximumLength(VendorCode)
                                    .WithMessage($"{nameof(UpdateAssetPurchaseDetailCommand.VendorCode)} {rule.Error}{VendorCode}");
                                purchase.RuleFor(x => x.VendorName)
                                     .MaximumLength(VendorName)
                                    .WithMessage($"{nameof(UpdateAssetPurchaseDetailCommand.VendorName)} {rule.Error}{VendorName}");
                                purchase.RuleFor(x => x.PoNo.ToString())
                                     .MaximumLength(PoNo)
                                    .WithMessage($"{nameof(UpdateAssetPurchaseDetailCommand.PoNo)} {rule.Error}{PoNo}");
                                purchase.RuleFor(x => x.PoSno.ToString())
                                     .MaximumLength(PoSno)
                                    .WithMessage($"{nameof(UpdateAssetPurchaseDetailCommand.PoSno)} {rule.Error}{PoSno}");
                                purchase.RuleFor(x => x.ItemCode)
                                     .MaximumLength(ItemCode)
                                    .WithMessage($"{nameof(UpdateAssetPurchaseDetailCommand.ItemCode)} {rule.Error}{ItemCode}");
                                purchase.RuleFor(x => x.ItemName)
                                     .MaximumLength(ItemName)
                                    .WithMessage($"{nameof(UpdateAssetPurchaseDetailCommand.ItemName)} {rule.Error}{ItemName}");
                                purchase.RuleFor(x => x.GrnNo.ToString())
                                     .MaximumLength(GrnNo)
                                    .WithMessage($"{nameof(UpdateAssetPurchaseDetailCommand.GrnNo)} {rule.Error}{GrnNo}");
                                purchase.RuleFor(x => x.GrnSno.ToString())
                                     .MaximumLength(GrnSno)
                                    .WithMessage($"{nameof(UpdateAssetPurchaseDetailCommand.GrnSno)} {rule.Error}{GrnSno}");
                                purchase.RuleFor(x => x.BillNo!.ToString())
                                     .MaximumLength(BillNo)
                                    .WithMessage($"{nameof(UpdateAssetPurchaseDetailCommand.BillNo)} {rule.Error}{BillNo}");

                    purchase.RuleFor(x => x.PjYear!.ToString())
                                     .MaximumLength(PjYear)
                                    .WithMessage($"{nameof(UpdateAssetPurchaseDetailCommand.PjYear)} {rule.Error}{PjYear}");
                     purchase.RuleFor(x => x.PjDocId!.ToString())
                                     .MaximumLength(PjDocId)
                                    .WithMessage($"{nameof(UpdateAssetPurchaseDetailCommand.PjDocId)} {rule.Error}{PjDocId}");
                     purchase.RuleFor(x => x.PjDocNo.ToString())
                                     .MaximumLength(PjDocNo)
                                    .WithMessage($"{nameof(UpdateAssetPurchaseDetailCommand.PjDocNo)} {rule.Error}{PjDocNo}");
                            });
                        break;
                    case "NumericOnly":
                        RuleFor(x => x.AssetMaster!.Quantity)
                        .InclusiveBetween(1, int.MaxValue)
                        .WithMessage($"{nameof(UpdateAssetMasterGeneralCommand.AssetMaster.Quantity)} {rule.Error}");                       
                        break;                  
                    default:                        
                        break;
                }
            }
        }
    }
}