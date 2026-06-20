using FluentValidation;
using FinanceManagement.Application.AccountGroup.Commands.MoveAccountGroup;
using FinanceManagement.Application.Common.Interfaces.IAccountGroup;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.AccountGroup
{
    public class MoveAccountGroupCommandValidator : AbstractValidator<MoveAccountGroupCommand>
    {
        private const int MinJustificationLength = 10;

        private readonly List<ValidationRule> _validationRules;
        private readonly IAccountGroupQueryRepository _queryRepository;

        public MoveAccountGroupCommandValidator(IAccountGroupQueryRepository queryRepository)
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
                        RuleFor(x => x.Justification)
                            .NotNull().WithMessage($"{nameof(MoveAccountGroupCommand.Justification)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(MoveAccountGroupCommand.Justification)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Account Group {rule.Error}");
                        break;

                    default:
                        break;
                }
            }

            // Documented justification — required for the statutory presentation change.
            RuleFor(x => x.Justification)
                .MinimumLength(MinJustificationLength)
                .WithMessage($"Justification must be at least {MinJustificationLength} characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Justification));

            // Approvers are NOT supplied here — the workflow engine routes the request through the
            // configured multilevel chain (Finance Controller → CFO).

            // New parent must be supplied and must exist / be active.
            RuleFor(x => x.NewParentAccountGroupId)
                .GreaterThan(0).WithMessage("New parent group is required.")
                .MustAsync(async (parentId, ct) => await _queryRepository.ParentExistsAsync(parentId))
                .WithMessage("Selected parent group does not exist or is inactive.")
                .When(x => x.NewParentAccountGroupId > 0);

            // A group cannot be moved under itself.
            RuleFor(x => x.NewParentAccountGroupId)
                .Must((cmd, parentId) => parentId != cmd.Id)
                .WithMessage("A group cannot be moved under itself.")
                .When(x => x.NewParentAccountGroupId > 0);

            // Circular-hierarchy guard — cannot move a group inside its own descendant.
            RuleFor(x => x.NewParentAccountGroupId)
                .MustAsync(async (cmd, parentId, ct) => !await _queryRepository.IsDescendantAsync(cmd.Id, parentId))
                .WithMessage("This move would place a group inside its own descendant — choose a different parent.")
                .When(x => x.Id > 0 && x.NewParentAccountGroupId > 0);

            // New parent must sit exactly one level above the group being moved
            // (so the moved group keeps its level and its subtree stays valid).
            RuleFor(x => x.NewParentAccountGroupId)
                .MustAsync(async (cmd, parentId, ct) =>
                {
                    var nodeLevel = await _queryRepository.GetLevelAsync(cmd.Id);
                    var parentLevel = await _queryRepository.GetLevelAsync(parentId);
                    return nodeLevel.HasValue && parentLevel.HasValue && parentLevel.Value == nodeLevel.Value - 1;
                })
                .WithMessage("New parent must be exactly one level above the group being moved.")
                .When(x => x.Id > 0 && x.NewParentAccountGroupId > 0);
        }
    }
}
