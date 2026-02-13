using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FAM.Application.AssetCategories.Command.UpdateAssetCategories;
using FAM.Presentation.Validation.Common;
using FluentValidation;
using Serilog;
using Shared.Validation.Common;

namespace FAM.Presentation.Validation.AssetCategories
{
    public class UpdateAssetCategoriesCommandValidator: AbstractValidator<UpdateAssetCategoriesCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        public UpdateAssetCategoriesCommandValidator(MaxLengthProvider maxLengthProvider)
        {
            var SortOrderMaxLength = maxLengthProvider.GetMaxLength<FAM.Domain.Entities.AssetCategories>("SortOrder") ?? 4;
            var CategoryNameMaxLength = maxLengthProvider.GetMaxLength<FAM.Domain.Entities.AssetCategories>("CategoryName") ?? 50;
            var CategoryDescriptionMaxLength = maxLengthProvider.GetMaxLength<FAM.Domain.Entities.AssetCategories>("Description") ?? 250;
            var CategoryIdMaxLength = maxLengthProvider.GetMaxLength<FAM.Domain.Entities.AssetCategories>("AssetGroupId") ?? 4;

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
                        // RuleFor(x => x.SortOrder)
                        //     .NotEmpty()
                        //     .WithMessage($"{nameof(UpdateAssetCategoriesCommand.SortOrder)} {rule.Error}");
                        RuleFor(x => x.CategoryName)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateAssetCategoriesCommand.CategoryName)} {rule.Error}");
                        RuleFor(x => x.AssetGroupId)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateAssetCategoriesCommand.AssetGroupId)} {rule.Error}");
                        break;
                    case "MaxLength":
                        // Apply MaxLength validation using dynamic max length values
                        RuleFor(x => x.SortOrder.ToString())
                            .MaximumLength(SortOrderMaxLength)
                            .WithMessage($"{nameof(UpdateAssetCategoriesCommand.SortOrder)} {rule.Error} {SortOrderMaxLength}");
                        RuleFor(x => x.CategoryName)
                            .MaximumLength(CategoryNameMaxLength)
                            .WithMessage($"{nameof(UpdateAssetCategoriesCommand.CategoryName)} {rule.Error} {CategoryNameMaxLength}");
                        RuleFor(x => x.Description)
                            .MaximumLength(CategoryDescriptionMaxLength)
                            .WithMessage($"{nameof(UpdateAssetCategoriesCommand.Description)} {rule.Error} {CategoryDescriptionMaxLength}");
                        RuleFor(x => x.AssetGroupId.ToString())
                            .MaximumLength(CategoryIdMaxLength)
                            .WithMessage($"{nameof(UpdateAssetCategoriesCommand.AssetGroupId)} {rule.Error} {CategoryIdMaxLength}");
                        break;
                    case "AlphaNumericWithPunctuation":
                        RuleFor(x => x.CategoryName)
                            .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern))
                            .WithMessage($"{nameof(UpdateAssetCategoriesCommand.CategoryName)} {rule.Error}");
                        break;
                    case "AlphabeticOnly":
                         RuleFor(x => x.Description)
                        .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern))
                        .When(x => !string.IsNullOrEmpty(x.Description))
                        .WithMessage($"{nameof(UpdateAssetCategoriesCommand.Description)} {rule.Error}");
                        break;
                    case "NonNegativeInteger":
                        RuleFor(x => x.SortOrder.ToString())
                             .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern))
                            .WithMessage($"{nameof(UpdateAssetCategoriesCommand.SortOrder)} {rule.Error}");
                        break;
                    // case "Percentage":
                    //     RuleFor(x => x.GroupPercentage.ToString())
                    //         .Matches(new Regex(rule.Pattern))
                    //         .WithMessage($"GroupPercentage {rule.Error}");
                    //     break;
                    default:
                          // Handle unknown rule (log or throw)
                        Log.Information("Warning: Unknown rule '{Rule}' encountered.", rule.Rule);
                        break;
            }
        }
    }
}
}