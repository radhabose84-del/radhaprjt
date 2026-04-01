using FAM.Application.Common.Interfaces.IAssetSubGroup;
using FAM.Application.AssetSubGroup.Command.DeleteAssetSubGroup;
using FluentValidation;
using Shared.Validation.Common;

namespace FAM.Presentation.Validation.AssetSubGroup
{
    public class DeleteAssetSubGroupCommandValidator : AbstractValidator<DeleteAssetSubGroupCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IAssetSubGroupQueryRepository _assetSubGroupQueryRepository;
        public DeleteAssetSubGroupCommandValidator(IAssetSubGroupQueryRepository assetSubGroupQueryRepository)
        {
            _validationRules = ValidationRuleLoader.LoadValidationRules();
            _assetSubGroupQueryRepository = assetSubGroupQueryRepository;

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
                            .WithMessage($"{nameof(DeleteAssetSubGroupCommand.Id)} {rule.Error}");
                        break;
                    case "SoftDelete":
                        RuleFor(x => x.Id)
                            .MustAsync(async (Id, cancellation) => !await _assetSubGroupQueryRepository.SoftDeleteValidationAsync(Id))
                            .WithMessage("This master is linked with other records. You cannot delete this record.");
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
