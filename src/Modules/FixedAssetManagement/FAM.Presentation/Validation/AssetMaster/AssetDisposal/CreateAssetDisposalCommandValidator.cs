using FAM.Application.AssetMaster.AssetDisposal.Command.CreateAssetDisposal;
using FAM.Presentation.Validation.Common;
using FluentValidation;
using Serilog;
using Shared.Validation.Common;

namespace FAM.Presentation.Validation.AssetMaster.AssetDisposal
{
    public class CreateAssetDisposalCommandValidator : AbstractValidator<CreateAssetDisposalCommand>
    {
        
        private readonly List<ValidationRule> _validationRules;
        public CreateAssetDisposalCommandValidator(MaxLengthProvider maxLengthProvider)
        {
            var AssetId = maxLengthProvider.GetMaxLength<FAM.Domain.Entities.AssetMaster.AssetDisposal>("AssetId") ?? 10;
            var AssetPurchaseId = maxLengthProvider.GetMaxLength<FAM.Domain.Entities.AssetMaster.AssetDisposal>("AssetPurchaseId") ?? 10;
            var DisposalType = maxLengthProvider.GetMaxLength<FAM.Domain.Entities.AssetMaster.AssetDisposal>("DisposalType") ?? 10;
            var DisposalReason = maxLengthProvider.GetMaxLength<FAM.Domain.Entities.AssetMaster.AssetDisposal>("DisposalReason") ?? 500;
            
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
                            .WithMessage($"{nameof(CreateAssetDisposalCommand.AssetId)} {rule.Error}");
                        RuleFor(x => x.AssetPurchaseId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateAssetDisposalCommand.AssetPurchaseId)} {rule.Error}");
                        RuleFor(x => x.DisposalDate)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateAssetDisposalCommand.DisposalDate)} {rule.Error}");
                         RuleFor(x => x.DisposalType)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateAssetDisposalCommand.DisposalType)} {rule.Error}");
                        RuleFor(x => x.DisposalAmount)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateAssetDisposalCommand.DisposalAmount)} {rule.Error}"); 
                        break;

                        case "MaxLength":
                        RuleFor(x => x.AssetId.ToString())
                            .MaximumLength(AssetId)
                            .WithMessage($"{nameof(CreateAssetDisposalCommand.AssetId)} {rule.Error} {AssetId}");
                        RuleFor(x => x.AssetPurchaseId.ToString())
                            .MaximumLength(AssetPurchaseId)
                            .WithMessage($"{nameof(CreateAssetDisposalCommand.AssetPurchaseId)} {rule.Error} {AssetPurchaseId}");
                        RuleFor(x => x.DisposalType.ToString())
                            .MaximumLength(DisposalType)
                            .WithMessage($"{nameof(CreateAssetDisposalCommand.DisposalType)} {rule.Error} {DisposalType}");
                        RuleFor(x => x.DisposalReason)
                            .MaximumLength(DisposalReason)
                            .WithMessage($"{nameof(CreateAssetDisposalCommand.DisposalReason)} {rule.Error} {DisposalReason}");
                        break;

                          case "NumericWithDecimal":
                        RuleFor(x => x.DisposalAmount.ToString())
                            .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern))
                            .WithMessage($"{nameof(CreateAssetDisposalCommand.DisposalAmount)} {rule.Error}");
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