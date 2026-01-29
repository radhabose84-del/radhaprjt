using System.Text.RegularExpressions;
using FAM.Application.AssetSubGroup.Command.UpdateAssetSubGroup;
using FAM.API.Validation.Common;
using FluentValidation;
using Serilog;
using Shared.Validation.Common;

namespace FAM.API.Validation.AssetSubGroup
{
    public class UpdateAssetSubGroupCommandValidator : AbstractValidator<UpdateAssetSubGroupCommand>
    {
        private readonly List<ValidationRule> _validationRules;

        public UpdateAssetSubGroupCommandValidator(MaxLengthProvider maxLengthProvider)
        {
            var sortordermaxlength = maxLengthProvider.GetMaxLength<FAM.Domain.Entities.AssetSubGroup>("SortOrder") ?? 4;
            var GroupNameMaxLength = maxLengthProvider.GetMaxLength<FAM.Domain.Entities.AssetSubGroup>("SubGroupName") ?? 50;
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
                        RuleFor(x => x.SubGroupName)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateAssetSubGroupCommand.SubGroupName)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.SortOrder.ToString())
                            .MaximumLength(sortordermaxlength)
                            .WithMessage($"{nameof(UpdateAssetSubGroupCommand.SortOrder)} {rule.Error} {sortordermaxlength}");
                        RuleFor(x => x.SubGroupName)
                            .MaximumLength(GroupNameMaxLength)
                            .WithMessage($"{nameof(UpdateAssetSubGroupCommand.SubGroupName)} {rule.Error} {GroupNameMaxLength}");
                        break;
                    case "AlphaNumericWithPunctuation":
                        RuleFor(x => x.SubGroupName)
                            .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern))
                            .WithMessage($"{nameof(UpdateAssetSubGroupCommand.SubGroupName)} {rule.Error}");
                        break;

                    case "NonNegativeInteger":
                        RuleFor(x => x.SortOrder.ToString())
                             .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern))
                            .WithMessage($"{nameof(UpdateAssetSubGroupCommand.SortOrder)} {rule.Error}");
                        break;
                    case "Percentage":
                        RuleFor(x => x.SubGroupPercentage.ToString())
                            .Matches(new Regex(rule.Pattern))
                            .WithMessage($"SubGroupPercentage {rule.Error}");
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