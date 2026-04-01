using FAM.Application.Common.Interfaces.IUOM;
using FAM.Application.UOM.Command.DeleteUOM;
using FluentValidation;
using Shared.Validation.Common;

namespace FAM.Presentation.Validation.UOM
{
    public class DeleteUOMCommandValidator : AbstractValidator<DeleteUOMCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IUOMQueryRepository _uomQueryRepository;
        public DeleteUOMCommandValidator(IUOMQueryRepository uomQueryRepository)
        {
            _validationRules = ValidationRuleLoader.LoadValidationRules();
            _uomQueryRepository = uomQueryRepository;

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
                            .WithMessage($"{nameof(DeleteUOMCommand.Id)} {rule.Error}");
                        break;
                    case "SoftDelete":
                        RuleFor(x => x.Id)
                            .MustAsync(async (Id, cancellation) => !await _uomQueryRepository.SoftDeleteValidationAsync(Id))
                            .WithMessage("This master is linked with other records. You cannot delete this record.");
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
