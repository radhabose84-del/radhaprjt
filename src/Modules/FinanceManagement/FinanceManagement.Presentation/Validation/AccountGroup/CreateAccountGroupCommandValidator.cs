using FluentValidation;
using FinanceManagement.Application.AccountGroup.Commands.CreateAccountGroup;
using FinanceManagement.Application.Common.Interfaces.IAccountGroup;
using FinanceManagement.Application.Common.Interfaces.IAccountTypeMaster;
using FinanceManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.AccountGroup
{
    public class CreateAccountGroupCommandValidator : AbstractValidator<CreateAccountGroupCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IAccountGroupQueryRepository _queryRepository;
        private readonly IAccountTypeMasterQueryRepository _accountTypeRepository;

        public CreateAccountGroupCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IAccountGroupQueryRepository queryRepository,
            IAccountTypeMasterQueryRepository accountTypeRepository)
        {
            _queryRepository = queryRepository;
            _accountTypeRepository = accountTypeRepository;

            var maxLengthCode = maxLengthProvider.GetMaxLength<Domain.Entities.AccountGroup>("GroupCode") ?? 50;
            var maxLengthName = maxLengthProvider.GetMaxLength<Domain.Entities.AccountGroup>("GroupName") ?? 150;

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
                        RuleFor(x => x.GroupCode)
                            .NotNull().WithMessage($"{nameof(CreateAccountGroupCommand.GroupCode)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateAccountGroupCommand.GroupCode)} {rule.Error}");

                        RuleFor(x => x.GroupName)
                            .NotNull().WithMessage($"{nameof(CreateAccountGroupCommand.GroupName)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateAccountGroupCommand.GroupName)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.GroupCode)
                            .MaximumLength(maxLengthCode)
                            .WithMessage($"{nameof(CreateAccountGroupCommand.GroupCode)} {rule.Error} {maxLengthCode} characters.");

                        RuleFor(x => x.GroupName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(CreateAccountGroupCommand.GroupName)} {rule.Error} {maxLengthName} characters.");
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.GroupCode)
                            .MustAsync(async (code, ct) => !await _queryRepository.AlreadyExistsAsync(code))
                            .WithMessage($"{nameof(CreateAccountGroupCommand.GroupCode)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.GroupCode));
                        break;

                    default:
                        break;
                }
            }

            // ── Hierarchy business rules (bespoke — not covered by shared JSON rules) ──
            // CompanyId is taken from the session token in the handler — no client-supplied value to validate.

            // Level 1 (no parent) must reference a statutory head from AccountTypeMaster.
            RuleFor(x => x.AccountTypeId)
                .NotNull().WithMessage("Account Type is required for a Level 1 group.")
                .When(x => !x.ParentAccountGroupId.HasValue);

            // ...and that head must actually exist (only checked once a value is supplied).
            RuleFor(x => x.AccountTypeId)
                .MustAsync(async (accountTypeId, ct) => !await _accountTypeRepository.NotFoundAsync(accountTypeId!.Value))
                .WithMessage("Selected Account Type does not exist.")
                .When(x => !x.ParentAccountGroupId.HasValue && x.AccountTypeId.HasValue);

            // Below Level 1, AccountType must not be set (it is carried only by the root head).
            RuleFor(x => x.AccountTypeId)
                .Null().WithMessage("Account Type applies only to Level 1 groups.")
                .When(x => x.ParentAccountGroupId.HasValue);

            // Parent must exist and be active.
            RuleFor(x => x.ParentAccountGroupId)
                .MustAsync(async (parentId, ct) => await _queryRepository.ParentExistsAsync(parentId!.Value))
                .WithMessage("Selected parent group does not exist or is inactive.")
                .When(x => x.ParentAccountGroupId.HasValue);

            // Parent must not already be a leaf (cannot nest below the configured max depth).
            RuleFor(x => x.ParentAccountGroupId)
                .MustAsync(async (parentId, ct) =>
                {
                    var level = await _queryRepository.GetLevelAsync(parentId!.Value);
                    return level.HasValue && level.Value < Domain.Entities.AccountGroup.DefaultMaxDepth;
                })
                .WithMessage($"Cannot create a sub-group under a Level {Domain.Entities.AccountGroup.DefaultMaxDepth} (leaf) group.")
                .When(x => x.ParentAccountGroupId.HasValue);
        }
    }
}
