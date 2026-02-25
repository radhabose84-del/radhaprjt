using FAM.Application.Common.Interfaces.IDepreciationGroup;
using FAM.Application.DepreciationGroup.Commands.DeleteDepreciationGroup;
using FluentValidation;
using Shared.Validation.Common;

namespace FAM.Presentation.Validation.DepreciationGroup
{
    public class DeleteDepreciationGroupCommandValidator : AbstractValidator<DeleteDepreciationGroupCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IDepreciationGroupQueryRepository _assetQueryRepository;
        public DeleteDepreciationGroupCommandValidator( IDepreciationGroupQueryRepository assetQueryRepository)
        {
            _validationRules = ValidationRuleLoader.LoadValidationRules();
            _assetQueryRepository = assetQueryRepository;

            
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
                            .WithMessage($"{nameof(DeleteDepreciationGroupCommand.Id)} {rule.Error}");
                        break;
                    case "SoftDelete":
                         RuleFor(x => x.Id)
                      .MustAsync(async (Id, cancellation) => !await _assetQueryRepository.SoftDeleteValidation(Id))
                        .WithMessage($"{rule.Error}");
                        break;
                    default:                        
                        break;
                }
            }
            
        }
    }
}