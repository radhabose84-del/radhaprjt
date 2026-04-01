using FAM.Application.Common.Interfaces.IAssetCategories;
using FAM.Application.AssetCategories.Command.DeleteAssetCategories;
using FluentValidation;
using Shared.Validation.Common;

namespace FAM.Presentation.Validation.AssetCategories
{
    public class DeleteAssetCategoriesCommandValidator : AbstractValidator<DeleteAssetCategoriesCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IAssetCategoriesQueryRepository _assetCategoriesQueryRepository;
        public DeleteAssetCategoriesCommandValidator(IAssetCategoriesQueryRepository assetCategoriesQueryRepository)
        {
            _validationRules = ValidationRuleLoader.LoadValidationRules();
            _assetCategoriesQueryRepository = assetCategoriesQueryRepository;

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
                            .WithMessage($"{nameof(DeleteAssetCategoriesCommand.Id)} {rule.Error}");
                        break;
                    case "SoftDelete":
                        RuleFor(x => x.Id)
                            .MustAsync(async (Id, cancellation) => !await _assetCategoriesQueryRepository.SoftDeleteValidationAsync(Id))
                            .WithMessage("This master is linked with other records. You cannot delete this record.");
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
