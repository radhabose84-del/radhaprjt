using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.AssetGroup.Command.CreateAssetGroup;
using FAM.API.Validation.Common;
using FluentValidation;
using Serilog;
using Shared.Validation.Common;

namespace FAM.API.Validation.AssetGroup
{
    public class CreateAssetGroupCommandValidator : AbstractValidator<CreateAssetGroupCommand>
    {
        private readonly List<ValidationRule> _validationRules;

        public CreateAssetGroupCommandValidator(MaxLengthProvider maxLengthProvider)
        {
            var CodeMaxLength = maxLengthProvider.GetMaxLength<FAM.Domain.Entities.AssetGroup>("Code") ?? 10;
            var GroupNameMaxLength = maxLengthProvider.GetMaxLength<FAM.Domain.Entities.AssetGroup>("GroupName") ?? 50;
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
                            .WithMessage($"{nameof(CreateAssetGroupCommand.Code)} {rule.Error}");
                        RuleFor(x => x.GroupName)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateAssetGroupCommand.GroupName)} {rule.Error}");
                        break;
                    case "MaxLength":
                        RuleFor(x => x.Code)
                            .MaximumLength(CodeMaxLength)
                            .WithMessage($"{nameof(CreateAssetGroupCommand.Code)} {rule.Error} {CodeMaxLength}");
                        RuleFor(x => x.GroupName)
                            .MaximumLength(GroupNameMaxLength)
                            .WithMessage($"{nameof(CreateAssetGroupCommand.GroupName)} {rule.Error} {GroupNameMaxLength}");
                        break;
                    case "AlphanumericOnly":
                              RuleFor(x => x.Code)
                             .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern)) 
                             .WithMessage($"{nameof(CreateAssetGroupCommand.Code)} {rule.Error}");   
                        break;
                    case "AlphaNumericWithPunctuation":
                        RuleFor(x => x.GroupName)
                            .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern))
                            .WithMessage($"{nameof(CreateAssetGroupCommand.GroupName)} {rule.Error}");
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