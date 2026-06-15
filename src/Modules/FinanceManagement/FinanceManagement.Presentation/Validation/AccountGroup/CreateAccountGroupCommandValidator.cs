using FluentValidation;
using FinanceManagement.Application.AccountGroup.Commands.CreateAccountGroup;
using FinanceManagement.Application.Common.Interfaces.IAccountGroup;
using FinanceManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.AccountGroup
{
    public class CreateAccountGroupCommandValidator : AbstractValidator<CreateAccountGroupCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IAccountGroupQueryRepository _queryRepository;

        public CreateAccountGroupCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IAccountGroupQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

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

            // Company is mandatory.
            RuleFor(x => x.CompanyId)
                .GreaterThan(0)
                .WithMessage("CompanyId is required.");

            // Level 1 (no parent) names are restricted to the statutory heads.
            // TODO: source this from AccountTypeMaster when that feature is built.
            RuleFor(x => x.GroupName)
                .Must(name => Domain.Entities.AccountGroup.Level1GroupNames
                    .Any(n => string.Equals(n, name, StringComparison.OrdinalIgnoreCase)))
                .WithMessage("Level 1 groups must be one of: Assets, Liabilities, Equity, Revenue, Expenses.")
                .When(x => !x.ParentAccountGroupId.HasValue && !string.IsNullOrWhiteSpace(x.GroupName));

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
