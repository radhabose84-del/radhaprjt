using FinanceManagement.Application.Common.Interfaces.ICurrencyForexConfig;
using FinanceManagement.Application.CurrencyForexConfig.Commands.DeleteCurrencyForexConfig;
using FluentValidation;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.CurrencyForexConfig
{
    public class DeleteCurrencyForexConfigCommandValidator : AbstractValidator<DeleteCurrencyForexConfigCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ICurrencyForexConfigQueryRepository _queryRepository;

        public DeleteCurrencyForexConfigCommandValidator(ICurrencyForexConfigQueryRepository queryRepository)
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
                            .WithMessage($"{nameof(DeleteCurrencyForexConfigCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Currency Forex Config {rule.Error}");
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
