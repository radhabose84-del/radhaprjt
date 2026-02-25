using FAM.Application.AssetSubCategories.Command.UpdateAssetSubCategories;
using FAM.Presentation.Validation.Common;
using FluentValidation;
using Serilog;
using Shared.Validation.Common;

namespace FAM.Presentation.Validation.AssetSubCategories
{
    public class UpdateAssetSubCategoriesCommandValidator : AbstractValidator<UpdateAssetSubCategoriesCommand>
    {
          private readonly List<ValidationRule> _validationRules;
        public UpdateAssetSubCategoriesCommandValidator(MaxLengthProvider maxLengthProvider)
        {
            var SortOrderMaxLength = maxLengthProvider.GetMaxLength<FAM.Domain.Entities.AssetSubCategories>("SortOrder") ?? 4;
            var CategoryNameMaxLength = maxLengthProvider.GetMaxLength<FAM.Domain.Entities.AssetSubCategories>("SubCategoryName") ?? 50;
            var CategoryDescriptionMaxLength = maxLengthProvider.GetMaxLength<FAM.Domain.Entities.AssetSubCategories>("Description") ?? 250;
            var CategoryIdMaxLength = maxLengthProvider.GetMaxLength<FAM.Domain.Entities.AssetSubCategories>("AssetCategoriesId") ?? 4;
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
                        RuleFor(x => x.SubCategoryName)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateAssetSubCategoriesCommand.SubCategoryName)} {rule.Error}");
                        RuleFor(x => x.AssetCategoriesId)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateAssetSubCategoriesCommand.AssetCategoriesId)} {rule.Error}");
                        break;
                    case "MaxLength":
                        // Apply MaxLength validation using dynamic max length values
                        RuleFor(x => x.SortOrder.ToString())
                            .MaximumLength(SortOrderMaxLength)
                            .WithMessage($"{nameof(UpdateAssetSubCategoriesCommand.SortOrder)} {rule.Error} {SortOrderMaxLength}");
                        RuleFor(x => x.SubCategoryName)
                            .MaximumLength(CategoryNameMaxLength)
                            .WithMessage($"{nameof(UpdateAssetSubCategoriesCommand.SubCategoryName)} {rule.Error} {CategoryNameMaxLength}");
                        RuleFor(x => x.Description)
                            .MaximumLength(CategoryDescriptionMaxLength)
                            .WithMessage($"{nameof(UpdateAssetSubCategoriesCommand.Description)} {rule.Error} {CategoryDescriptionMaxLength}");
                        RuleFor(x => x.AssetCategoriesId.ToString())
                            .MaximumLength(CategoryIdMaxLength)
                            .WithMessage($"{nameof(UpdateAssetSubCategoriesCommand.AssetCategoriesId)} {rule.Error} {CategoryIdMaxLength}");
                        break;
                    case "AlphaNumericWithPunctuation":
                        RuleFor(x => x.SubCategoryName)
                            .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern))
                            .WithMessage($"{nameof(UpdateAssetSubCategoriesCommand.SubCategoryName)} {rule.Error}");
                        break;
                    case "AlphabeticOnly":
                         RuleFor(x => x.Description)
                        .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern))
                        .When(x => !string.IsNullOrEmpty(x.Description))
                        .WithMessage($"{nameof(UpdateAssetSubCategoriesCommand.Description)} {rule.Error}");
                        break;
                    case "NonNegativeInteger":
                        RuleFor(x => x.SortOrder.ToString())
                             .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern))
                            .WithMessage($"{nameof(UpdateAssetSubCategoriesCommand.SortOrder)} {rule.Error}");
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