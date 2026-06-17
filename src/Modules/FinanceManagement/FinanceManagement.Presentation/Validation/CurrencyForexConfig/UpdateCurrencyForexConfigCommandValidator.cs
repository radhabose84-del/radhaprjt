using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.ICurrencyForexConfig;
using FinanceManagement.Application.CurrencyForexConfig.Commands.UpdateCurrencyForexConfig;
using FinanceManagement.Presentation.Validation.Common;
using FluentValidation;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.CurrencyForexConfig
{
    public class UpdateCurrencyForexConfigCommandValidator : AbstractValidator<UpdateCurrencyForexConfigCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ICurrencyForexConfigQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;

        public UpdateCurrencyForexConfigCommandValidator(
            MaxLengthProvider maxLengthProvider,
            ICurrencyForexConfigQueryRepository queryRepository,
            IIPAddressService ipAddressService)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;

            var maxLengthName = maxLengthProvider.GetMaxLength<FinanceManagement.Domain.Entities.CurrencyForexConfig>("CurrencyTypeName") ?? 100;

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
                        RuleFor(x => x.CurrencyTypeName)
                            .NotNull()
                            .WithMessage($"{nameof(UpdateCurrencyForexConfigCommand.CurrencyTypeName)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateCurrencyForexConfigCommand.CurrencyTypeName)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.CurrencyTypeName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(UpdateCurrencyForexConfigCommand.CurrencyTypeName)} {rule.Error} {maxLengthName} characters.");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Currency Forex Config {rule.Error}");
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.CurrencyTypeName)
                            .MustAsync(async (cmd, name, ct) =>
                            {
                                var companyId = _ipAddressService.GetCompanyId() ?? 0;
                                return !await _queryRepository.AlreadyExistsByNameAsync(name!, companyId, cmd.Id);
                            })
                            .WithMessage($"{nameof(UpdateCurrencyForexConfigCommand.CurrencyTypeName)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.CurrencyTypeName));
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateCurrencyForexConfigCommand.IsActive)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
