using FAM.Application.AssetGroup.Command.UpdateAssetGroup;
using FAM.Presentation.Validation.Common;
using FluentValidation;
using Serilog;
using Shared.Validation.Common;

namespace FAM.Presentation.Validation.AssetGroup
{
    public class UpdateAssetGroupCommandValidator : AbstractValidator<UpdateAssetGroupCommand>
    {
        private readonly List<ValidationRule> _validationRules;

        public UpdateAssetGroupCommandValidator(MaxLengthProvider maxLengthProvider)
        {
            var sortordermaxlength = maxLengthProvider.GetMaxLength<FAM.Domain.Entities.AssetGroup>("SortOrder") ?? 4;
            var GroupNameMaxLength = maxLengthProvider.GetMaxLength<FAM.Domain.Entities.AssetGroup>("GroupName") ?? 50;
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
                        RuleFor(x => x.GroupName)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateAssetGroupCommand.GroupName)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.SortOrder.ToString())
                            .MaximumLength(sortordermaxlength)
                            .WithMessage($"{nameof(UpdateAssetGroupCommand.SortOrder)} {rule.Error} {sortordermaxlength}");
                        RuleFor(x => x.GroupName)
                            .MaximumLength(GroupNameMaxLength)
                            .WithMessage($"{nameof(UpdateAssetGroupCommand.GroupName)} {rule.Error} {GroupNameMaxLength}");
                        break;
                    case "Alphanumeric":
                        RuleFor(x => x.GroupName)
                            .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern))
                            .WithMessage($"{nameof(UpdateAssetGroupCommand.GroupName)} {rule.Error}");
                        break;

                    case "NonNegativeInteger":
                        RuleFor(x => x.SortOrder.ToString())
                             .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern))
                            .WithMessage($"{nameof(UpdateAssetGroupCommand.SortOrder)} {rule.Error}");
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