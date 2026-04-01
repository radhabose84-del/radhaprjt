using FAM.Application.Common.Interfaces.IManufacture;
using FAM.Application.Manufacture.Commands.DeleteManufacture;
using FluentValidation;
using Shared.Validation.Common;

namespace FAM.Presentation.Validation.Manufacture
{
    public class DeleteManufactureCommandValidator : AbstractValidator<DeleteManufactureCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IManufactureQueryRepository _manufactureQueryRepository;
        public DeleteManufactureCommandValidator(IManufactureQueryRepository manufactureQueryRepository)
        {
            _validationRules = ValidationRuleLoader.LoadValidationRules();
            _manufactureQueryRepository = manufactureQueryRepository;

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
                            .WithMessage($"{nameof(DeleteManufactureCommand.Id)} {rule.Error}");
                        break;
                    case "SoftDelete":
                        RuleFor(x => x.Id)
                            .MustAsync(async (Id, cancellation) => !await _manufactureQueryRepository.SoftDeleteValidationAsync(Id))
                            .WithMessage("This master is linked with other records. You cannot delete this record.");
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
