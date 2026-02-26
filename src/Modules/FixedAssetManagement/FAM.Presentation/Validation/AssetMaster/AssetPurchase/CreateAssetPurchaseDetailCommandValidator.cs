using FAM.Application.AssetMaster.AssetPurchase.Commands.CreateAssetPurchaseDetails;
using FAM.Domain.Entities.AssetPurchase;
using FAM.Presentation.Validation.Common;
using FluentValidation;
using Serilog;
using Shared.Validation.Common;

namespace FAM.Presentation.Validation.AssetMaster.AssetPurchase
{
    public class CreateAssetPurchaseDetailCommandValidator : AbstractValidator<CreateAssetPurchaseDetailCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        public CreateAssetPurchaseDetailCommandValidator(MaxLengthProvider maxLengthProvider)
        {
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
                        RuleFor(x => x.OldUnitId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateAssetPurchaseDetailCommand.OldUnitId)} {rule.Error}");
                        RuleFor(x => x.VendorCode)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateAssetPurchaseDetailCommand.VendorCode)} {rule.Error}");
                        RuleFor(x => x.VendorName)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateAssetPurchaseDetailCommand.VendorName)} {rule.Error}");
                        RuleFor(x => x.PoNo)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateAssetPurchaseDetailCommand.PoNo)} {rule.Error}");
                        RuleFor(x => x.PoSno)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateAssetPurchaseDetailCommand.PoSno)} {rule.Error}");
                        RuleFor(x => x.ItemCode)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateAssetPurchaseDetailCommand.ItemCode)} {rule.Error}");
                        RuleFor(x => x.ItemName)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateAssetPurchaseDetailCommand.ItemName)} {rule.Error}");
                        RuleFor(x => x.GrnNo)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateAssetPurchaseDetailCommand.GrnNo)} {rule.Error}");
                        RuleFor(x => x.GrnSno)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateAssetPurchaseDetailCommand.GrnSno)} {rule.Error}");
                        RuleFor(x => x.AcceptedQty)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateAssetPurchaseDetailCommand.AcceptedQty)} {rule.Error}");
                        RuleFor(x => x.PurchaseValue)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateAssetPurchaseDetailCommand.PurchaseValue)} {rule.Error}");
                        RuleFor(x => x.GrnValue)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateAssetPurchaseDetailCommand.GrnValue)} {rule.Error}");
                        RuleFor(x => x.BillNo)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateAssetPurchaseDetailCommand.BillNo)} {rule.Error}");
                        RuleFor(x => x.PjYear)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateAssetPurchaseDetailCommand.PjYear)} {rule.Error}");
                        RuleFor(x => x.PjDocId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateAssetPurchaseDetailCommand.PjDocId)} {rule.Error}");
                        RuleFor(x => x.PjDocNo)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateAssetPurchaseDetailCommand.PjDocNo)} {rule.Error}");
                        When(x => x.AssetId != 0, () =>
                        {
                            RuleFor(x => x.AssetId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateAssetPurchaseDetailCommand.AssetId)} is required.");
                        });                                                
                        RuleFor(x => x.AssetSourceId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateAssetPurchaseDetailCommand.AssetSourceId)} {rule.Error}");
                        
                        break;
                     case "MaxLength": 
                     // Add more cases for other rules as needed
                        RuleFor(x => x.VendorCode)
                            .MaximumLength(VendorCode)
                            .WithMessage($"{nameof(CreateAssetPurchaseDetailCommand.VendorCode)} {rule.Error} {VendorCode}");
                        RuleFor(x => x.VendorName)
                            .MaximumLength(VendorName)
                            .WithMessage($"{nameof(CreateAssetPurchaseDetailCommand.VendorName)} {rule.Error} {VendorName}");
                        RuleFor(x => x.PoNo.ToString())
                            .MaximumLength(PoNo)
                            .WithMessage($"{nameof(CreateAssetPurchaseDetailCommand.PoNo)} {rule.Error} {PoNo}");
                        RuleFor(x => x.PoSno.ToString())
                            .MaximumLength(PoSno)
                            .WithMessage($"{nameof(CreateAssetPurchaseDetailCommand.PoSno)} {rule.Error} {PoSno}");
                        RuleFor(x => x.ItemCode)
                            .MaximumLength(ItemCode)
                            .WithMessage($"{nameof(CreateAssetPurchaseDetailCommand.ItemCode)} {rule.Error} {ItemCode}");
                        RuleFor(x => x.ItemName)
                            .MaximumLength(ItemName)
                            .WithMessage($"{nameof(CreateAssetPurchaseDetailCommand.ItemName)} {rule.Error} {ItemName}");
                        RuleFor(x => x.GrnNo.ToString())
                            .MaximumLength(GrnNo)
                            .WithMessage($"{nameof(CreateAssetPurchaseDetailCommand.GrnNo)} {rule.Error} {GrnNo}");
                        RuleFor(x => x.GrnSno.ToString())
                            .MaximumLength(GrnSno)
                            .WithMessage($"{nameof(CreateAssetPurchaseDetailCommand.GrnSno)} {rule.Error} {GrnSno}");
                        RuleFor(x => x.BillNo.ToString())
                            .MaximumLength(BillNo)
                            .WithMessage($"{nameof(CreateAssetPurchaseDetailCommand.BillNo)} {rule.Error} {BillNo}");
                        RuleFor(x => x.PjYear.ToString())
                            .MaximumLength(PjYear)
                            .WithMessage($"{nameof(CreateAssetPurchaseDetailCommand.PjYear)} {rule.Error} {PjYear}");
                        RuleFor(x => x.PjDocId.ToString())
                            .MaximumLength(PjDocId)
                            .WithMessage($"{nameof(CreateAssetPurchaseDetailCommand.PjDocId)} {rule.Error} {PjDocId}");
                        RuleFor(x => x.PjDocNo.ToString())
                            .MaximumLength(PjDocNo)
                            .WithMessage($"{nameof(CreateAssetPurchaseDetailCommand.PjDocNo)} {rule.Error} {PjDocNo}");
                        RuleFor(x => x.AssetId.ToString())
                            .MaximumLength(AssetId)
                            .WithMessage($"{nameof(CreateAssetPurchaseDetailCommand.AssetId)} {rule.Error} {AssetId}");
                        RuleFor(x => x.AssetSourceId.ToString())
                            .MaximumLength(AssetSourceId)
                            .WithMessage($"{nameof(CreateAssetPurchaseDetailCommand.AssetSourceId)} {rule.Error} {AssetSourceId}");
                        break;
                    case "YesNoStatus":
                        RuleFor(x => x.QcCompleted)
                        .NotEmpty()
                        .Must(value => value.HasValue && System.Text.RegularExpressions.Regex.IsMatch(value.Value.ToString(), rule.Pattern))
                        .WithMessage($"{nameof(CreateAssetPurchaseDetailCommand.QcCompleted)} {rule.Error}");
                         break;

                        default:
                        // Handle unknown rule (log or throw)
                        Log.Information("Warning: Unknown rule '{Rule}' encountered.", rule.Rule);
                        break;
                }
            }
        }
    }
}    

    