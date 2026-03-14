using FluentValidation;
using ProductionManagement.Application.Common.Interfaces.IMiscTypeMaster;
using ProductionManagement.Application.MiscTypeMaster.Commands.DeleteMiscTypeMaster;
using Shared.Validation.Common;

namespace ProductionManagement.Presentation.Validation.MiscTypeMaster
{
    public class DeleteMiscTypeMasterCommandValidator : AbstractValidator<DeleteMiscTypeMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IMiscTypeMasterQueryRepository _queryRepository;

        public DeleteMiscTypeMasterCommandValidator(IMiscTypeMasterQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;
            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || _validationRules.Count == 0)
                throw new InvalidOperationException("Validation rules could not be loaded.");

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.Id)
                            .NotEmpty()
                            .WithMessage($"{nameof(DeleteMiscTypeMasterCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Misc Type Master {rule.Error}");
                        break;

                    case "SoftDelete":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.SoftDeleteValidationAsync(id))
                            .WithMessage("Misc Type Master cannot be deleted because it has associated Misc Master records.");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
