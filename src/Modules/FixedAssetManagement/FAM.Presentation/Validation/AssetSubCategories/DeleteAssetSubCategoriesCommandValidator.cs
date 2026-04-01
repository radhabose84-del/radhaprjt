using FAM.Application.Common.Interfaces.IAssetSubCategories;
using FAM.Application.AssetSubCategories.Command.DeleteAssetSubCategories;
using FluentValidation;
using Shared.Validation.Common;

namespace FAM.Presentation.Validation.AssetSubCategories
{
    public class DeleteAssetSubCategoriesCommandValidator : AbstractValidator<DeleteAssetSubCategoriesCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IAssetSubCategoriesQueryRepository _assetSubCategoriesQueryRepository;
        public DeleteAssetSubCategoriesCommandValidator(IAssetSubCategoriesQueryRepository assetSubCategoriesQueryRepository)
        {
            _validationRules = ValidationRuleLoader.LoadValidationRules();
            _assetSubCategoriesQueryRepository = assetSubCategoriesQueryRepository;

            if (_validationRules == null || !_validationRules.Any())
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.Id)
                            .NotEmpty()
                            .WithMessage($"{nameof(DeleteAssetSubCategoriesCommand.Id)} {rule.Error}");
                        break;
                    case "SoftDelete":
                        RuleFor(x => x.Id)
                            .MustAsync(async (Id, cancellation) => !await _assetSubCategoriesQueryRepository.SoftDeleteValidationAsync(Id))
                            .WithMessage("This master is linked with other records. You cannot delete this record.");
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
