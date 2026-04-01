using FAM.Application.Common.Interfaces.IAssetGroup;
using FAM.Application.AssetGroup.Command.DeleteAssetGroup;
using FluentValidation;
using Shared.Validation.Common;

namespace FAM.Presentation.Validation.AssetGroup
{
    public class DeleteAssetGroupCommandValidator : AbstractValidator<DeleteAssetGroupCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IAssetGroupQueryRepository _assetGroupQueryRepository;
        public DeleteAssetGroupCommandValidator(IAssetGroupQueryRepository assetGroupQueryRepository)
        {
            _validationRules = ValidationRuleLoader.LoadValidationRules();
            _assetGroupQueryRepository = assetGroupQueryRepository;

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
                            .WithMessage($"{nameof(DeleteAssetGroupCommand.Id)} {rule.Error}");
                        break;
                    case "SoftDelete":
                        RuleFor(x => x.Id)
                            .MustAsync(async (Id, cancellation) => !await _assetGroupQueryRepository.SoftDeleteValidationAsync(Id))
                            .WithMessage("This master is linked with other records. You cannot delete this record.");
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
