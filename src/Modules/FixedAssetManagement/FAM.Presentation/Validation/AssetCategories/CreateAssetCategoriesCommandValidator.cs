using FAM.Application.AssetCategories.Command.CreateAssetCategories;
using FAM.Application.Common.Interfaces.IAssetCategories;
using FAM.Presentation.Validation.Common;
using FluentValidation;
using Serilog;
using Shared.Validation.Common;

namespace FAM.Presentation.Validation.AssetCategories
{
    public class CreateAssetCategoriesCommandValidator : AbstractValidator<CreateAssetCategoriesCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IAssetCategoriesCommandRepository _iAssetCategoriesCommandRepository;

        public CreateAssetCategoriesCommandValidator(MaxLengthProvider maxLengthProvider, IAssetCategoriesCommandRepository iAssetCategoriesCommandRepository)
        {
            _iAssetCategoriesCommandRepository = iAssetCategoriesCommandRepository;
            var CodeMaxLength = maxLengthProvider.GetMaxLength<FAM.Domain.Entities.AssetCategories>("Code") ?? 10;
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
                        // RuleFor(x => x.Code)
                        //     .NotEmpty()
                        //     .WithMessage($"{nameof(CreateAssetCategoriesCommand.Code)} {rule.Error}");
                        RuleFor(x => x.CategoryName)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateAssetCategoriesCommand.CategoryName)} {rule.Error}");
                        RuleFor(x => x.AssetGroupId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateAssetCategoriesCommand.AssetGroupId)} {rule.Error}");
                        break;
                    case "MaxLength":
                        // RuleFor(x => x.Code)
                        //     .MaximumLength(CodeMaxLength)
                        //     .WithMessage($"{nameof(CreateAssetCategoriesCommand.Code)} {rule.Error} {CodeMaxLength}");
                        RuleFor(x => x.CategoryName)
                            .MaximumLength(CategoryNameMaxLength)
                            .WithMessage($"{nameof(CreateAssetCategoriesCommand.CategoryName)} {rule.Error} {CategoryNameMaxLength}");
                        RuleFor(x => x.Description)
                            .MaximumLength(CategoryDescriptionMaxLength)
                            .WithMessage($"{nameof(CreateAssetCategoriesCommand.Description)} {rule.Error} {CategoryDescriptionMaxLength}");
                        RuleFor(x => x.AssetGroupId.ToString())
                            .MaximumLength(CategoryIdMaxLength)
                            .WithMessage($"{nameof(CreateAssetCategoriesCommand.AssetGroupId)} {rule.Error} {CategoryIdMaxLength}");
                        break;
                    // case "AlphanumericOnly":
                    //           RuleFor(x => x.Code)
                    //          .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern)) 
                    //          .WithMessage($"{nameof(CreateAssetCategoriesCommand.Code)} {rule.Error}");   
                    //     break;
                    case "Alphanumeric":
                        RuleFor(x => x.CategoryName)
                            .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern))
                            .WithMessage($"{nameof(CreateAssetCategoriesCommand.CategoryName)} {rule.Error}");
                        break;
                    case "AlphabeticOnly":
                        RuleFor(x => x.Description)
                       .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern))
                       .When(x => !string.IsNullOrEmpty(x.Description))
                       .WithMessage($"{nameof(CreateAssetCategoriesCommand.Description)} {rule.Error}");
                        break;
                    // case "Percentage":
                    //     RuleFor(x => x.GroupPercentage.ToString())
                    //         .Matches(new Regex(rule.Pattern))
                    //         .WithMessage($"GroupPercentage {rule.Error}");
                    //     break;
                     case "AlreadyExists":
                        RuleFor(x => x.CategoryName)
                       .MustAsync(async (CategoryName, cancellation) => !await _iAssetCategoriesCommandRepository.ExistsByNameAsync(CategoryName ?? string.Empty, null))
                       .WithName("CategoryName")
                       .WithMessage("Asset Category name already exists.");
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
