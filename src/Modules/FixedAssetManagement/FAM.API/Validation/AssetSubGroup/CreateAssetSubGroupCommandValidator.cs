using System.Text.RegularExpressions;
using FAM.Application.AssetSubGroup.Command.CreateAssetSubGroup;
using FAM.Application.Common.Interfaces.IAssetSubGroup;
using FAM.API.Validation.Common;
using FluentValidation;
using Serilog;
using Shared.Validation.Common;

namespace FAM.API.Validation.AssetSubGroup
{
    public class CreateAssetSubGroupCommandValidator : AbstractValidator<CreateAssetSubGroupCommand>
    {
        private readonly List<ValidationRule> _validationRules;

        public CreateAssetSubGroupCommandValidator(MaxLengthProvider maxLengthProvider, IAssetSubGroupCommandRepository assetSubGroupCommandRepository)
        {
            var CodeMaxLength = maxLengthProvider.GetMaxLength<FAM.Domain.Entities.AssetSubGroup>("Code") ?? 10;
            var GroupNameMaxLength = maxLengthProvider.GetMaxLength<FAM.Domain.Entities.AssetSubGroup>("SubGroupName") ?? 50;
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
                        RuleFor(x => x.Code)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateAssetSubGroupCommand.Code)} {rule.Error}");
                        RuleFor(x => x.SubGroupName)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateAssetSubGroupCommand.SubGroupName)} {rule.Error}");
                        break;
                    case "MaxLength":
                        RuleFor(x => x.Code)
                            .MaximumLength(CodeMaxLength)
                            .WithMessage($"{nameof(CreateAssetSubGroupCommand.Code)} {rule.Error} {CodeMaxLength}");
                        RuleFor(x => x.SubGroupName)
                            .MaximumLength(GroupNameMaxLength)
                            .WithMessage($"{nameof(CreateAssetSubGroupCommand.SubGroupName)} {rule.Error} {GroupNameMaxLength}");
                        break;
                    case "AlphanumericOnly":
                        RuleFor(x => x.Code)
                       .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern))
                       .WithMessage($"{nameof(CreateAssetSubGroupCommand.Code)} {rule.Error}");
                        break;
                    case "AlphaNumericWithPunctuation":
                        RuleFor(x => x.SubGroupName)
                            .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern))
                            .WithMessage($"{nameof(CreateAssetSubGroupCommand.SubGroupName)} {rule.Error}");
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
            // ✅ Validate GroupId exists
            RuleFor(x => x.GroupId)
            .GreaterThan(0).WithMessage("GroupId must be greater than 0.")
            .MustAsync(async (groupId, cancellation) =>
                await assetSubGroupCommandRepository.ExistsAsync(groupId))
            .WithMessage("GroupId does not exist."); 
        }
     }
}