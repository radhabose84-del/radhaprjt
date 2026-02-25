using FAM.Application.AssetMaster.AssetAdditionalCost.Commands.UpdateAssetAdditionalCost;
using FAM.Presentation.Validation.Common;
using FluentValidation;
using Serilog;
using Shared.Validation.Common;

namespace FAM.Presentation.Validation.AssetMaster.AssetAdditionalCost
{
    public class UpdateAssetAdditionalCostCommandValidator : AbstractValidator<UpdateAssetAdditionalCostCommand>
    {
        private readonly List<ValidationRule> _validationRules;

        public UpdateAssetAdditionalCostCommandValidator(MaxLengthProvider maxLengthProvider)    
        {
            var JournalNo = maxLengthProvider.GetMaxLength<FAM.Domain.Entities.AssetPurchase.AssetAdditionalCost>("JournalNo") ?? 100;
            var CostType = maxLengthProvider.GetMaxLength<FAM.Domain.Entities.AssetPurchase.AssetAdditionalCost>("CostType") ?? 10;
               // Load validation rules from JSON or another source
            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || !_validationRules.Any())
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }
              foreach (var rule in _validationRules)
            {
                 switch (rule.Rule)
                {
                    case "NotEmpty":
                        // Apply NotEmpty validation
                        RuleFor(x => x.JournalNo)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateAssetAdditionalCostCommand.JournalNo)} {rule.Error}");
                         RuleFor(x => x.CostType)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateAssetAdditionalCostCommand.CostType)} {rule.Error}");
                        RuleFor(x => x.Amount)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateAssetAdditionalCostCommand.Amount)} {rule.Error}"); 
                        break;
                    case "MaxLength":
                        RuleFor(x => x.JournalNo)
                            .MaximumLength(JournalNo)
                            .WithMessage($"{nameof(UpdateAssetAdditionalCostCommand.JournalNo)} {rule.Error} {JournalNo}");
                        RuleFor(x => x.CostType.ToString())
                            .MaximumLength(CostType)
                            .WithMessage($"{nameof(UpdateAssetAdditionalCostCommand.CostType)} {rule.Error} {CostType}");
                        break;

                    case "NumericWithDecimal":
                        RuleFor(x => x.Amount.ToString())
                            .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern))
                            .WithMessage($"{nameof(UpdateAssetAdditionalCostCommand.Amount)} {rule.Error}");
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