using Contracts.Interfaces.Lookups.Users;
using FinanceManagement.Application.Common.Interfaces.IProfitCentre;
using FinanceManagement.Application.ProfitCentre.Commands.CreateProfitCentre;
using FinanceManagement.Presentation.Validation.Common;
using FluentValidation;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.ProfitCentre
{
    public class CreateProfitCentreCommandValidator : AbstractValidator<CreateProfitCentreCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IProfitCentreQueryRepository _queryRepository;
        private readonly IUserLookup _userLookup;

        public CreateProfitCentreCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IProfitCentreQueryRepository queryRepository,
            IUserLookup userLookup)
        {
            _queryRepository = queryRepository;
            _userLookup = userLookup;

            var maxLengthCode = maxLengthProvider.GetMaxLength<FinanceManagement.Domain.Entities.ProfitCentre>("ProfitCentreCode") ?? 20;
            var maxLengthName = maxLengthProvider.GetMaxLength<FinanceManagement.Domain.Entities.ProfitCentre>("ProfitCentreName") ?? 150;

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
                        RuleFor(x => x.ProfitCentreCode)
                            .NotNull().WithMessage($"{nameof(CreateProfitCentreCommand.ProfitCentreCode)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateProfitCentreCommand.ProfitCentreCode)} {rule.Error}");

                        RuleFor(x => x.ProfitCentreName)
                            .NotNull().WithMessage($"{nameof(CreateProfitCentreCommand.ProfitCentreName)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateProfitCentreCommand.ProfitCentreName)} {rule.Error}");
                        break;

                    case "CodeWithHyphen":
                        // PC codes contain hyphens (e.g. PC-SPIN-001) — alphanumeric + hyphen, no spaces.
                        RuleFor(x => x.ProfitCentreCode)
                            .Matches(rule.Pattern)
                            .WithMessage($"{nameof(CreateProfitCentreCommand.ProfitCentreCode)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.ProfitCentreCode));
                        break;

                    case "MaxLength":
                        RuleFor(x => x.ProfitCentreCode)
                            .MaximumLength(maxLengthCode)
                            .WithMessage($"{nameof(CreateProfitCentreCommand.ProfitCentreCode)} {rule.Error} {maxLengthCode} characters.");

                        RuleFor(x => x.ProfitCentreName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(CreateProfitCentreCommand.ProfitCentreName)} {rule.Error} {maxLengthName} characters.");
                        break;

                    case "AlreadyExists":
                        // Uniqueness is global — the code may not be reused in any company (AC#2).
                        RuleFor(x => x.ProfitCentreCode)
                            .MustAsync(async (code, ct) => !await _queryRepository.AlreadyExistsByCodeAsync(code!))
                            .WithMessage($"{nameof(CreateProfitCentreCommand.ProfitCentreCode)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.ProfitCentreCode));
                        break;

                    default:
                        break;
                }
            }

            // ── Hierarchy & cross-module FK rules (business-specific — no shared JSON rule applies) ──

            // Level must be a valid PROFITCENTRELEVEL misc row.
            RuleFor(x => x.LevelId)
                .GreaterThan(0).WithMessage("Level is required.")
                .MustAsync(async (levelId, ct) => await _queryRepository.GetLevelSortOrderAsync(levelId) > 0)
                .WithMessage("Invalid Level.")
                .When(x => x.LevelId > 0);

            // Parent must be exactly one level above (null for L1 Segment, an L1 for L2 Sub-segment).
            RuleFor(x => x.ParentProfitCentreId)
                .MustAsync(async (cmd, parentId, ct) =>
                    await _queryRepository.ParentValidForLevelAsync(parentId, cmd.LevelId))
                .WithMessage("Invalid Parent Segment — the parent must be exactly one level above.")
                .When(x => x.LevelId > 0);

            // Responsible Head — optional, but must be a valid user when supplied.
            RuleFor(x => x.ResponsibleHeadId)
                .MustAsync(async (headId, ct) => await _userLookup.GetByIdAsync(headId!.Value) != null)
                .WithMessage("A valid Responsible Head is required.")
                .When(x => x.ResponsibleHeadId.HasValue && x.ResponsibleHeadId.Value > 0);
        }
    }
}
