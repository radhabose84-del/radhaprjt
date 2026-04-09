using FluentValidation;
using SalesManagement.Application.Common.Interfaces.ICommissionSplit;
using SalesManagement.Application.CommissionSplit.Commands.CreateCommissionSplit;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.CommissionSplit
{
    public class CreateCommissionSplitCommandValidator : AbstractValidator<CreateCommissionSplitCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ICommissionSplitQueryRepository _queryRepo;

        public CreateCommissionSplitCommandValidator(
            MaxLengthProvider maxLengthProvider,
            ICommissionSplitQueryRepository queryRepo)
        {
            _queryRepo = queryRepo;

            var maxLengthName = maxLengthProvider.GetMaxLength<Domain.Entities.CommissionSplit>("SplitName") ?? 100;

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
                        RuleFor(x => x.SplitName)
                            .NotNull()
                            .WithMessage($"{nameof(CreateCommissionSplitCommand.SplitName)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateCommissionSplitCommand.SplitName)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.SplitName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(CreateCommissionSplitCommand.SplitName)} {rule.Error} {maxLengthName} characters.");
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.SplitName)
                            .MustAsync(async (name, ct) => !await _queryRepo.AlreadyExistsAsync(name!))
                            .WithMessage($"{nameof(CreateCommissionSplitCommand.SplitName)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.SplitName));
                        break;

                    case "FKColumnDelete":
                        // Validate RoleId and ShareTypeId exist in MiscMaster for each detail row
                        RuleForEach(x => x.Details).ChildRules(detail =>
                        {
                            detail.RuleFor(d => d.RoleId)
                                .MustAsync(async (id, ct) => await _queryRepo.MiscMasterExistsAsync(id))
                                .WithMessage($"RoleId {rule.Error}")
                                .When(d => d.RoleId > 0);

                            detail.RuleFor(d => d.ShareTypeId)
                                .MustAsync(async (id, ct) => await _queryRepo.MiscMasterExistsAsync(id))
                                .WithMessage($"ShareTypeId {rule.Error}")
                                .When(d => d.ShareTypeId > 0);
                        }).When(x => x.Details != null && x.Details.Count > 0);
                        break;

                    case "GreaterThan":
                        // ShareValue must be > 0 for each detail row
                        RuleForEach(x => x.Details).ChildRules(detail =>
                        {
                            detail.RuleFor(d => d.ShareValue)
                                .GreaterThan(0)
                                .WithMessage($"ShareValue {rule.Error}");

                            detail.RuleFor(d => d.RoleId)
                                .GreaterThan(0)
                                .WithMessage($"RoleId {rule.Error}");

                            detail.RuleFor(d => d.ShareTypeId)
                                .GreaterThan(0)
                                .WithMessage($"ShareTypeId {rule.Error}");
                        }).When(x => x.Details != null && x.Details.Count > 0);
                        break;

                    default:
                        break;
                }
            }

            // ── Business Rules (outside switch — not in validation-rules.json) ──

            // Must have exactly 2 detail rows
            RuleFor(x => x.Details)
                .Must(d => d != null && d.Count == 2)
                .WithMessage("Exactly two split configuration rows are required (Agent and Sub-Agent).");

            // No duplicate roles
            RuleFor(x => x.Details)
                .Must(d => d != null && d.Select(r => r.RoleId).Distinct().Count() == d.Count)
                .WithMessage("Duplicate roles are not allowed.")
                .When(x => x.Details != null && x.Details.Count > 0);

            // All rows must have the same ShareType
            RuleFor(x => x.Details)
                .Must(d => d != null && d.Select(r => r.ShareTypeId).Distinct().Count() == 1)
                .WithMessage("All rows in a split must use the same Share Type.")
                .When(x => x.Details != null && x.Details.Count > 0);

            // If ShareType = Percentage → sum must equal 100
            // Query MiscMaster code to determine if the ShareType is "PERCENTAGE"
            RuleFor(x => x.Details)
                .MustAsync(async (details, ct) =>
                {
                    if (details == null || details.Count == 0) return true;

                    var shareTypeId = details.First().ShareTypeId;
                    var code = await _queryRepo.GetMiscMasterCodeAsync(shareTypeId);

                    // Only enforce sum=100 for Percentage share type
                    if (string.Equals(code, "PERCENTAGE", StringComparison.OrdinalIgnoreCase))
                    {
                        return details.Sum(r => r.ShareValue) == 100;
                    }

                    return true; // Fixed type — no sum constraint
                })
                .WithMessage(x =>
                {
                    var sum = x.Details?.Sum(r => r.ShareValue) ?? 0;
                    return $"Percentage total must equal 100%. Current total: {sum}%.";
                })
                .When(x => x.Details != null && x.Details.Count > 0 &&
                           x.Details.Select(r => r.ShareTypeId).Distinct().Count() == 1);
        }
    }
}
