using FluentValidation;
using SalesManagement.Application.Common.Interfaces.ICommissionSplit;
using SalesManagement.Application.CommissionSplit.Commands.DeleteCommissionSplit;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.CommissionSplit
{
    public class DeleteCommissionSplitCommandValidator : AbstractValidator<DeleteCommissionSplitCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ICommissionSplitQueryRepository _queryRepository;

        public DeleteCommissionSplitCommandValidator(ICommissionSplitQueryRepository queryRepository)
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
                            .WithMessage($"{nameof(DeleteCommissionSplitCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"CommissionSplit {rule.Error}");
                        break;

                    case "SoftDelete":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.SoftDeleteValidationAsync(id))
                            .WithMessage("This master is linked with other records. You cannot delete this record.");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
