using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.ICurrencyForexConfig;
using FinanceManagement.Application.CurrencyForexConfig.Commands.CreateCurrencyForexConfig;
using FinanceManagement.Presentation.Validation.Common;
using FluentValidation;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.CurrencyForexConfig
{
    public class CreateCurrencyForexConfigCommandValidator : AbstractValidator<CreateCurrencyForexConfigCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ICurrencyForexConfigQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;

        public CreateCurrencyForexConfigCommandValidator(
            MaxLengthProvider maxLengthProvider,
            ICurrencyForexConfigQueryRepository queryRepository,
            IIPAddressService ipAddressService)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;

            var maxLengthCode = maxLengthProvider.GetMaxLength<FinanceManagement.Domain.Entities.CurrencyForexConfig>("CurrencyTypeCode") ?? 20;
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
                        RuleFor(x => x.CurrencyTypeCode)
                            .NotNull()
                            .WithMessage($"{nameof(CreateCurrencyForexConfigCommand.CurrencyTypeCode)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateCurrencyForexConfigCommand.CurrencyTypeCode)} {rule.Error}");

                        RuleFor(x => x.CurrencyTypeName)
                            .NotNull()
                            .WithMessage($"{nameof(CreateCurrencyForexConfigCommand.CurrencyTypeName)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateCurrencyForexConfigCommand.CurrencyTypeName)} {rule.Error}");
                        break;

                    case "Alphanumeric":
                        RuleFor(x => x.CurrencyTypeCode)
                            .Matches(rule.Pattern)
                            .WithMessage($"{nameof(CreateCurrencyForexConfigCommand.CurrencyTypeCode)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.CurrencyTypeCode));
                        break;

                    case "MaxLength":
                        RuleFor(x => x.CurrencyTypeCode)
                            .MaximumLength(maxLengthCode)
                            .WithMessage($"{nameof(CreateCurrencyForexConfigCommand.CurrencyTypeCode)} {rule.Error} {maxLengthCode} characters.");

                        RuleFor(x => x.CurrencyTypeName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(CreateCurrencyForexConfigCommand.CurrencyTypeName)} {rule.Error} {maxLengthName} characters.");
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.CurrencyTypeCode)
                            .MustAsync(async (code, ct) =>
                            {
                                var companyId = _ipAddressService.GetCompanyId() ?? 0;
                                return !await _queryRepository.AlreadyExistsByCodeAsync(code!, companyId);
                            })
                            .WithMessage($"{nameof(CreateCurrencyForexConfigCommand.CurrencyTypeCode)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.CurrencyTypeCode));

                        RuleFor(x => x.CurrencyTypeName)
                            .MustAsync(async (name, ct) =>
                            {
                                var companyId = _ipAddressService.GetCompanyId() ?? 0;
                                return !await _queryRepository.AlreadyExistsByNameAsync(name!, companyId);
                            })
                            .WithMessage($"{nameof(CreateCurrencyForexConfigCommand.CurrencyTypeName)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.CurrencyTypeName));
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
