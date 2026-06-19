using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IVoucherTypeMaster;
using FinanceManagement.Application.VoucherType.Commands.CreateVoucherType;
using FinanceManagement.Presentation.Validation.Common;
using FluentValidation;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.VoucherType
{
    public class CreateVoucherTypeCommandValidator : AbstractValidator<CreateVoucherTypeCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IVoucherTypeMasterQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;

        public CreateVoucherTypeCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IVoucherTypeMasterQueryRepository queryRepository,
            IIPAddressService ipAddressService)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;

            var maxLengthCode = maxLengthProvider.GetMaxLength<FinanceManagement.Domain.Entities.VoucherTypeMaster>("VoucherTypeCode") ?? 10;
            var maxLengthName = maxLengthProvider.GetMaxLength<FinanceManagement.Domain.Entities.VoucherTypeMaster>("VoucherTypeName") ?? 100;

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
                        RuleFor(x => x.VoucherTypeCode)
                            .NotNull().WithMessage($"{nameof(CreateVoucherTypeCommand.VoucherTypeCode)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateVoucherTypeCommand.VoucherTypeCode)} {rule.Error}");

                        RuleFor(x => x.VoucherTypeName)
                            .NotNull().WithMessage($"{nameof(CreateVoucherTypeCommand.VoucherTypeName)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateVoucherTypeCommand.VoucherTypeName)} {rule.Error}");

                        RuleFor(x => x.NumberPadding)
                            .InclusiveBetween(1, 10)
                            .WithMessage($"{nameof(CreateVoucherTypeCommand.NumberPadding)} must be between 1 and 10.");

                        RuleFor(x => x.AllowedAccountTypeIds)
                            .NotNull().WithMessage($"{nameof(CreateVoucherTypeCommand.AllowedAccountTypeIds)} {rule.Error}")
                            .Must(ids => ids != null && ids.Count > 0)
                            .WithMessage("At least one allowed account type is required.");
                        break;

                    case "Alphanumeric":
                        RuleFor(x => x.VoucherTypeCode)
                            .Matches(rule.Pattern)
                            .WithMessage($"{nameof(CreateVoucherTypeCommand.VoucherTypeCode)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.VoucherTypeCode));
                        break;

                    case "MaxLength":
                        RuleFor(x => x.VoucherTypeCode)
                            .MaximumLength(maxLengthCode)
                            .WithMessage($"{nameof(CreateVoucherTypeCommand.VoucherTypeCode)} {rule.Error} {maxLengthCode} characters.");

                        RuleFor(x => x.VoucherTypeName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(CreateVoucherTypeCommand.VoucherTypeName)} {rule.Error} {maxLengthName} characters.");
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x)
                            .MustAsync(async (command, ct) =>
                            {
                                var companyId = _ipAddressService.GetCompanyId() ?? 0;
                                foreach (var accountTypeId in command.AllowedAccountTypeIds.Distinct())
                                {
                                    if (!await _queryRepository.AccountTypeExistsAsync(accountTypeId, companyId))
                                        return false;
                                }
                                return true;
                            })
                            .WithMessage($"{nameof(CreateVoucherTypeCommand.AllowedAccountTypeIds)} {rule.Error}")
                            .When(x => x.AllowedAccountTypeIds != null && x.AllowedAccountTypeIds.Count > 0);
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.VoucherTypeCode)
                            .MustAsync(async (code, ct) =>
                                !await _queryRepository.AlreadyExistsByCodeAsync(code!, _ipAddressService.GetCompanyId() ?? 0))
                            .WithMessage($"{nameof(CreateVoucherTypeCommand.VoucherTypeCode)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.VoucherTypeCode));
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
