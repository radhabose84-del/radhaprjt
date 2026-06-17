using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IGlAccountMaster;
using FinanceManagement.Application.GlAccountMaster.Commands.UpdateGlAccountMaster;
using FinanceManagement.Presentation.Validation.Common;
using FluentValidation;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.GlAccountMaster
{
    public class UpdateGlAccountMasterCommandValidator : AbstractValidator<UpdateGlAccountMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IGlAccountMasterQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;

        public UpdateGlAccountMasterCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IGlAccountMasterQueryRepository queryRepository,
            IIPAddressService ipAddressService)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;

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
                            .WithMessage($"{nameof(UpdateGlAccountMasterCommand.AccountTypeId)} {rule.Error}");

                        RuleFor(x => x.AccountGroupId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateGlAccountMasterCommand.AccountGroupId)} {rule.Error}");

                        RuleFor(x => x.AccountName)
                            .NotNull()
                            .WithMessage($"{nameof(UpdateGlAccountMasterCommand.AccountName)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateGlAccountMasterCommand.AccountName)} {rule.Error}");

                        RuleFor(x => x.NormalBalanceId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateGlAccountMasterCommand.NormalBalanceId)} {rule.Error}");

                        RuleFor(x => x.CurrencyTypeId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateGlAccountMasterCommand.CurrencyTypeId)} {rule.Error}");

                        RuleFor(x => x.SubLedgerTypeId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateGlAccountMasterCommand.SubLedgerTypeId)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.AccountName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(UpdateGlAccountMasterCommand.AccountName)} {rule.Error} {maxLengthName} characters.");

                        RuleFor(x => x.Description)
                            .MaximumLength(maxLengthDescription)
                            .WithMessage($"{nameof(UpdateGlAccountMasterCommand.Description)} {rule.Error} {maxLengthDescription} characters.");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) =>
                                !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"GL Account {rule.Error}");
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
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.AccountName)
                            .MustAsync(async (cmd, name, ct) =>
                            {
                                var companyId = _ipAddressService.GetCompanyId() ?? 0;
                                return !await _queryRepository.AlreadyExistsByNameAsync(name!, companyId, cmd.Id);
                            })
                            .WithMessage($"{nameof(UpdateGlAccountMasterCommand.AccountName)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.AccountName));
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateGlAccountMasterCommand.IsActive)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
