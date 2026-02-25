#nullable disable
using FluentValidation;
using SalesManagement.Application.Common.Interfaces.IBusinessUnit;
using SalesManagement.Application.BusinessUnit.Commands.DeleteBusinessUnit;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.BusinessUnit
{
    public class DeleteBusinessUnitCommandValidator : AbstractValidator<DeleteBusinessUnitCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IBusinessUnitQueryRepository _queryRepository;

        public DeleteBusinessUnitCommandValidator(IBusinessUnitQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;
            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || _validationRules.Count == 0)
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
                            .WithMessage($"{nameof(DeleteBusinessUnitCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Business Unit {rule.Error}");
                        break;

                    case "SoftDelete":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.SoftDeleteValidationAsync(id))
                            .WithMessage(rule.Error);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
