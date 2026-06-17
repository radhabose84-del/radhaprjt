using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IGlAccountMaster;
using FinanceManagement.Application.GlAccountMaster.Commands.CreateGlAccountMaster;
using FinanceManagement.Presentation.Validation.Common;
using FluentValidation;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.GlAccountMaster
{
    public class CreateGlAccountMasterCommandValidator : AbstractValidator<CreateGlAccountMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IGlAccountMasterQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;

        public CreateGlAccountMasterCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IGlAccountMasterQueryRepository queryRepository,
            IIPAddressService ipAddressService)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;

            var maxLengthCode = maxLengthProvider.GetMaxLength<FinanceManagement.Domain.Entities.GlAccountMaster>("AccountCode") ?? 20;
            var maxLengthName = maxLengthProvider.GetMaxLength<FinanceManagement.Domain.Entities.GlAccountMaster>("AccountName") ?? 200;
            var maxLengthDescription = maxLengthProvider.GetMaxLength<FinanceManagement.Domain.Entities.GlAccountMaster>("Description") ?? 500;

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
                        RuleFor(x => x.AccountTypeId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateGlAccountMasterCommand.AccountTypeId)} {rule.Error}");

                        RuleFor(x => x.AccountGroupId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateGlAccountMasterCommand.AccountGroupId)} {rule.Error}");

                        RuleFor(x => x.AccountCode)
                            .NotNull()
                            .WithMessage($"{nameof(CreateGlAccountMasterCommand.AccountCode)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateGlAccountMasterCommand.AccountCode)} {rule.Error}");

                        RuleFor(x => x.AccountName)
                            .NotNull()
                            .WithMessage($"{nameof(CreateGlAccountMasterCommand.AccountName)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateGlAccountMasterCommand.AccountName)} {rule.Error}");

                        RuleFor(x => x.NormalBalanceId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateGlAccountMasterCommand.NormalBalanceId)} {rule.Error}");

                        RuleFor(x => x.CurrencyTypeId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateGlAccountMasterCommand.CurrencyTypeId)} {rule.Error}");

                        RuleFor(x => x.SubLedgerTypeId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateGlAccountMasterCommand.SubLedgerTypeId)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.AccountCode)
                            .MaximumLength(maxLengthCode)
                            .WithMessage($"{nameof(CreateGlAccountMasterCommand.AccountCode)} {rule.Error} {maxLengthCode} characters.");

                        RuleFor(x => x.AccountName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(CreateGlAccountMasterCommand.AccountName)} {rule.Error} {maxLengthName} characters.");

                        RuleFor(x => x.Description)
                            .MaximumLength(maxLengthDescription)
                            .WithMessage($"{nameof(CreateGlAccountMasterCommand.Description)} {rule.Error} {maxLengthDescription} characters.");
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.AccountTypeId)
                            .MustAsync(async (typeId, ct) =>
                            {
                                var companyId = _ipAddressService.GetCompanyId() ?? 0;
                                return await _queryRepository.AccountTypeExistsForCompanyAsync(typeId, companyId);
                            })
                            .WithMessage("Account Type not configured for this company.")
                            .When(x => x.AccountTypeId > 0);

                        RuleFor(x => x.AccountGroupId)
                            .MustAsync(async (groupId, ct) =>
                            {
                                var companyId = _ipAddressService.GetCompanyId() ?? 0;
                                return await _queryRepository.AccountGroupExistsForCompanyAsync(groupId, companyId);
                            })
                            .WithMessage("Account Group not configured for this company.")
                            .When(x => x.AccountGroupId > 0);

                        // AC2 — accounts attach only at a leaf group.
                        RuleFor(x => x.AccountGroupId)
                            .MustAsync(async (groupId, ct) =>
                            {
                                var companyId = _ipAddressService.GetCompanyId() ?? 0;
                                return await _queryRepository.AccountGroupIsLeafForCompanyAsync(groupId, companyId);
                            })
                            .WithMessage("Accounts attach only at leaf level — select a leaf group.")
                            .When(x => x.AccountGroupId > 0);

                        RuleFor(x => x.NormalBalanceId)
                            .MustAsync(async (nbId, ct) => await _queryRepository.NormalBalanceExistsAsync(nbId))
                            .WithMessage("Invalid Normal Balance.")
                            .When(x => x.NormalBalanceId > 0);

                        RuleFor(x => x.SubLedgerTypeId)
                            .MustAsync(async (sltId, ct) => await _queryRepository.SubLedgerTypeExistsAsync(sltId))
                            .WithMessage("Invalid Sub-Ledger Type.")
                            .When(x => x.SubLedgerTypeId > 0);

                        // Currency Type dropdown -> CurrencyForexConfig master (US-GL02-12)
                        RuleFor(x => x.CurrencyTypeId)
                            .MustAsync(async (ctId, ct) =>
                            {
                                var companyId = _ipAddressService.GetCompanyId() ?? 0;
                                return await _queryRepository.CurrencyTypeExistsForCompanyAsync(ctId, companyId);
                            })
                            .WithMessage("Invalid Currency Type.")
                            .When(x => x.CurrencyTypeId > 0);
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.AccountCode)
                            .MustAsync(async (code, ct) =>
                            {
                                var companyId = _ipAddressService.GetCompanyId() ?? 0;
                                return !await _queryRepository.AlreadyExistsByCodeAsync(code!, companyId);
                            })
                            .WithMessage($"{nameof(CreateGlAccountMasterCommand.AccountCode)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.AccountCode));

                        RuleFor(x => x.AccountName)
                            .MustAsync(async (name, ct) =>
                            {
                                var companyId = _ipAddressService.GetCompanyId() ?? 0;
                                return !await _queryRepository.AlreadyExistsByNameAsync(name!, companyId);
                            })
                            .WithMessage($"{nameof(CreateGlAccountMasterCommand.AccountName)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.AccountName));
                        break;

                    default:
                        break;
                }
            }

            // Account code must be digits only
            RuleFor(x => x.AccountCode)
                .Matches("^[0-9]+$")
                .WithMessage($"{nameof(CreateGlAccountMasterCommand.AccountCode)} must be digits only.")
                .When(x => !string.IsNullOrWhiteSpace(x.AccountCode));

            // Account code length + StartCode prefix must match the chosen AccountType
            RuleFor(x => x)
                .CustomAsync(async (cmd, ctx, ct) =>
                {
                    if (cmd.AccountTypeId <= 0 || string.IsNullOrWhiteSpace(cmd.AccountCode))
                        return;

                    var fmt = await _queryRepository.GetAccountTypeFormatAsync(cmd.AccountTypeId);
                    if (fmt == null) return;

                    var (codeLen, startCode, typeName) = fmt.Value;

                    if (cmd.AccountCode!.Length != codeLen)
                    {
                        ctx.AddFailure(nameof(cmd.AccountCode),
                            $"Account Code must be exactly {codeLen} digits for {typeName}.");
                        return;
                    }

                    if (!string.IsNullOrEmpty(startCode) && cmd.AccountCode[0] != startCode[0])
                    {
                        ctx.AddFailure(nameof(cmd.AccountCode),
                            $"Account Code must start with {startCode} for {typeName}.");
                    }
                });
        }
    }
}
