using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.AssetMaster.AssetAdditionalCost.Commands.CreateAssetAdditionalCost;
using FAM.API.Validation.Common;
using FluentValidation;
using Serilog;
using Shared.Validation.Common;

namespace FAM.API.Validation.AssetMaster.AssetAdditionalCost
{
    public class CreateAssetAdditionalCostCommandValidator : AbstractValidator<CreateAssetAdditionalCostCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        public CreateAssetAdditionalCostCommandValidator(MaxLengthProvider maxLengthProvider)
        {
            var AssetId = maxLengthProvider.GetMaxLength<FAM.Domain.Entities.AssetPurchase.AssetAdditionalCost>("AssetId") ?? 10;
            var AssetSourceId = maxLengthProvider.GetMaxLength<FAM.Domain.Entities.AssetPurchase.AssetAdditionalCost>("AssetSourceId") ?? 10;
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
                        RuleFor(x => x.AssetId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateAssetAdditionalCostCommand.AssetId)} {rule.Error}");
                        RuleFor(x => x.AssetSourceId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateAssetAdditionalCostCommand.AssetSourceId)} {rule.Error}");
                        RuleFor(x => x.JournalNo)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateAssetAdditionalCostCommand.JournalNo)} {rule.Error}");
                         RuleFor(x => x.CostType)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateAssetAdditionalCostCommand.CostType)} {rule.Error}");
                        RuleFor(x => x.Amount)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateAssetAdditionalCostCommand.Amount)} {rule.Error}"); 
                        break;
                    case "MaxLength":
                        RuleFor(x => x.AssetId.ToString())
                            .MaximumLength(AssetId)
                            .WithMessage($"{nameof(CreateAssetAdditionalCostCommand.AssetId)} {rule.Error} {AssetId}");
                        RuleFor(x => x.AssetSourceId.ToString())
                            .MaximumLength(AssetSourceId)
                            .WithMessage($"{nameof(CreateAssetAdditionalCostCommand.AssetSourceId)} {rule.Error} {AssetSourceId}");
                        RuleFor(x => x.JournalNo)
                            .MaximumLength(JournalNo)
                            .WithMessage($"{nameof(CreateAssetAdditionalCostCommand.JournalNo)} {rule.Error} {JournalNo}");
                        RuleFor(x => x.CostType.ToString())
                            .MaximumLength(CostType)
                            .WithMessage($"{nameof(CreateAssetAdditionalCostCommand.CostType)} {rule.Error} {CostType}");
                        break;

                    case "NumericWithDecimal":
                        RuleFor(x => x.Amount.ToString())
                            .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern))
                            .WithMessage($"{nameof(CreateAssetAdditionalCostCommand.Amount)} {rule.Error}");
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