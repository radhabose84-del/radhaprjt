using UserManagement.Application.Common.Interfaces.IUnit;
using UserManagement.Application.Units.Commands.DeleteUnit;
using FluentValidation;
using Shared.Validation.Common;

namespace UserManagement.Presentation.Validation.Unit
{
    public class DeleteUnitCommandValidator : AbstractValidator<DeleteUnitCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IUnitQueryRepository _unitQueryRepository;

        public DeleteUnitCommandValidator(IUnitQueryRepository unitQueryRepository)
        {
            _unitQueryRepository = unitQueryRepository;
            _validationRules = ValidationRuleLoader.LoadValidationRules();

            if (_validationRules == null || !_validationRules.Any())
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.UnitId)
                            .NotEmpty()
                            .WithMessage($"{nameof(DeleteUnitCommand.UnitId)} {rule.Error}");
                        break;

                    case "SoftDelete":
                        RuleFor(x => x.UnitId)
                            .MustAsync(async (id, ct) => !await _unitQueryRepository.SoftDeleteValidationAsync(id))
                            .WithMessage("This master is linked with other records. You cannot delete this record.");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
