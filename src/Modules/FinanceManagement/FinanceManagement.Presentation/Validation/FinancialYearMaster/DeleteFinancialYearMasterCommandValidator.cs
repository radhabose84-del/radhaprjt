using FinanceManagement.Application.Common.Interfaces.IFinancialYearMaster;
using FinanceManagement.Application.FinancialYearMaster.Commands.DeleteFinancialYearMaster;
using FluentValidation;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.FinancialYearMaster
{
    public class DeleteFinancialYearMasterCommandValidator : AbstractValidator<DeleteFinancialYearMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IFinancialYearMasterQueryRepository _queryRepository;

        public DeleteFinancialYearMasterCommandValidator(IFinancialYearMasterQueryRepository queryRepository)
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
                            .WithMessage($"{nameof(DeleteFinancialYearMasterCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Financial Year {rule.Error}");
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
