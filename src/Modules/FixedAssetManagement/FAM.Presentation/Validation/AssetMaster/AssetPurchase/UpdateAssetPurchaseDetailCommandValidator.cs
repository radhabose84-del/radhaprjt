using FAM.Application.AssetMaster.AssetPurchase.Commands.UpdateAssetPurchaseDetails;
using FAM.Domain.Entities.AssetPurchase;
using FAM.Presentation.Validation.Common;
using FluentValidation;
using Serilog;
using Shared.Validation.Common;

namespace FAM.Presentation.Validation.AssetMaster.AssetPurchase
{
    public class UpdateAssetPurchaseDetailCommandValidator: AbstractValidator<UpdateAssetPurchaseDetailCommand>
    {
        private readonly List<ValidationRule> _validationRules;

        public UpdateAssetPurchaseDetailCommandValidator(MaxLengthProvider maxLengthProvider)
        {
            var BudgetType = maxLengthProvider.GetMaxLength<AssetPurchaseDetails>("BudgetType") ?? 50;
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
            var PjYear = maxLengthProvider.GetMaxLength<AssetPurchaseDetails>("PjYear") ?? 8;
            var PjDocId = maxLengthProvider.GetMaxLength<AssetPurchaseDetails>("PjDocId") ?? 40;
            var PjDocSr = maxLengthProvider.GetMaxLength<AssetPurchaseDetails>("PjDocSr") ?? 40;
            var PjDocNo = maxLengthProvider.GetMaxLength<AssetPurchaseDetails>("PjDocNo") ?? 10;
            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || !_validationRules.Any())
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }
           // Loop through the rules and apply them
            foreach (var rule in _validationRules)
            {
                 switch (rule.Rule)
                {
                    case "NotEmpty":
                        // Apply NotEmpty validation
                        RuleFor(x => x.VendorCode)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateAssetPurchaseDetailCommand.VendorCode)} {rule.Error}");
                        RuleFor(x => x.VendorName)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateAssetPurchaseDetailCommand.VendorName)} {rule.Error}");
                        RuleFor(x => x.PoNo)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateAssetPurchaseDetailCommand.PoNo)} {rule.Error}");
                        RuleFor(x => x.PoSno)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateAssetPurchaseDetailCommand.PoSno)} {rule.Error}");
                        RuleFor(x => x.ItemCode)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateAssetPurchaseDetailCommand.ItemCode)} {rule.Error}");
                        RuleFor(x => x.ItemName)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateAssetPurchaseDetailCommand.ItemName)} {rule.Error}");
                        RuleFor(x => x.GrnNo)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateAssetPurchaseDetailCommand.GrnNo)} {rule.Error}");
                        RuleFor(x => x.GrnSno)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateAssetPurchaseDetailCommand.GrnSno)} {rule.Error}");
                          RuleFor(x => x.AcceptedQty)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateAssetPurchaseDetailCommand.AcceptedQty)} {rule.Error}");
                        RuleFor(x => x.PurchaseValue)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateAssetPurchaseDetailCommand.PurchaseValue)} {rule.Error}");
                        RuleFor(x => x.GrnValue)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateAssetPurchaseDetailCommand.GrnValue)} {rule.Error}");
                        RuleFor(x => x.BillNo)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateAssetPurchaseDetailCommand.BillNo)} {rule.Error}");
                        RuleFor(x => x.PjYear)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateAssetPurchaseDetailCommand.PjYear)} {rule.Error}");
                        RuleFor(x => x.PjDocId)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateAssetPurchaseDetailCommand.PjDocId)} {rule.Error}");
                        RuleFor(x => x.PjDocNo)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateAssetPurchaseDetailCommand.PjDocNo)} {rule.Error}");
                        break;
                     case "MaxLength": 
                     // Add more cases for other rules as needed
                        RuleFor(x => x.VendorCode)
                            .MaximumLength(VendorCode)
                            .WithMessage($"{nameof(UpdateAssetPurchaseDetailCommand.VendorCode)} {rule.Error} {VendorCode}");
                        RuleFor(x => x.VendorName)
                            .MaximumLength(VendorName)
                            .WithMessage($"{nameof(UpdateAssetPurchaseDetailCommand.VendorName)} {rule.Error} {VendorName}");
                        RuleFor(x => x.PoNo.ToString())
                            .MaximumLength(PoNo)
                            .WithMessage($"{nameof(UpdateAssetPurchaseDetailCommand.PoNo)} {rule.Error} {PoNo}");
                        RuleFor(x => x.PoSno.ToString())
                            .MaximumLength(PoSno)
                            .WithMessage($"{nameof(UpdateAssetPurchaseDetailCommand.PoSno)} {rule.Error} {PoSno}");
                        RuleFor(x => x.ItemCode)
                            .MaximumLength(ItemCode)
                            .WithMessage($"{nameof(UpdateAssetPurchaseDetailCommand.ItemCode)} {rule.Error} {ItemCode}");
                        RuleFor(x => x.ItemName)
                            .MaximumLength(ItemName)
                            .WithMessage($"{nameof(UpdateAssetPurchaseDetailCommand.ItemName)} {rule.Error} {ItemName}");
                        RuleFor(x => x.GrnNo.ToString())
                            .MaximumLength(GrnNo)
                            .WithMessage($"{nameof(UpdateAssetPurchaseDetailCommand.GrnNo)} {rule.Error} {GrnNo}");
                        RuleFor(x => x.GrnSno.ToString())
                            .MaximumLength(GrnSno)
                            .WithMessage($"{nameof(UpdateAssetPurchaseDetailCommand.GrnSno)} {rule.Error} {GrnSno}");
                        RuleFor(x => x.BillNo!.ToString())
                            .MaximumLength(BillNo)
                            .WithMessage($"{nameof(UpdateAssetPurchaseDetailCommand.BillNo)} {rule.Error} {BillNo}");
                        RuleFor(x => x.PjYear!.ToString())
                            .MaximumLength(PjYear)
                            .WithMessage($"{nameof(UpdateAssetPurchaseDetailCommand.PjYear)} {rule.Error} {PjYear}");
                        RuleFor(x => x.PjDocId!.ToString())
                            .MaximumLength(PjDocId)
                            .WithMessage($"{nameof(UpdateAssetPurchaseDetailCommand.PjDocId)} {rule.Error} {PjDocId}");
                        RuleFor(x => x.PjDocNo.ToString())
                            .MaximumLength(PjDocNo)
                            .WithMessage($"{nameof(UpdateAssetPurchaseDetailCommand.PjDocNo)} {rule.Error} {PjDocNo}");
                        break;

                        case "YesNoStatus":
                        RuleFor(x => x.QcCompleted)
                        .NotEmpty()
                        .Must(value => value.HasValue && System.Text.RegularExpressions.Regex.IsMatch(value.Value.ToString(), rule.Pattern))
                        .WithMessage($"{nameof(UpdateAssetPurchaseDetailCommand.QcCompleted)} {rule.Error}");
                         break;
                          // Handle unknown rule (log or throw)
                        default:
                        Log.Information("Warning: Unknown rule '{Rule}' encountered.", rule.Rule);
                        break;
                }
            }


    }
}        
    }
