using UserManagement.Application.Common.Interfaces.ICurrency;
using UserManagement.Application.Currency.Commands.DeleteCurrency;
using FluentValidation;
using Shared.Validation.Common;

namespace UserManagement.Presentation.Validation.Currency
{
    public class DeleteCurrencyCommandValidator : AbstractValidator<DeleteCurrencyCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ICurrencyQueryRepository _currencyQueryRepository;

        public DeleteCurrencyCommandValidator(ICurrencyQueryRepository currencyQueryRepository)
        {
            _currencyQueryRepository = currencyQueryRepository;
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
                        RuleFor(x => x.Id)
                            .NotEmpty()
                            .WithMessage($"{nameof(DeleteCurrencyCommand.Id)} {rule.Error}");
                        break;

                    case "SoftDelete":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _currencyQueryRepository.SoftDeleteValidationAsync(id))
                            .WithMessage("This master is linked with other records. You cannot delete this record.");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
