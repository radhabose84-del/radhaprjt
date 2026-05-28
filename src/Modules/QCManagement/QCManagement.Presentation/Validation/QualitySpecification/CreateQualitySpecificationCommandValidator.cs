using Contracts.Interfaces.Lookups.Inventory;
using FluentValidation;
using QCManagement.Application.Common.Interfaces.IQualitySpecification;
using QCManagement.Application.Common.Interfaces.IQualityTemplate;
using QCManagement.Application.QualitySpecification.Commands.CreateQualitySpecification;
using QCManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace QCManagement.Presentation.Validation.QualitySpecification
{
    public class CreateQualitySpecificationCommandValidator : AbstractValidator<CreateQualitySpecificationCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IQualitySpecificationQueryRepository _queryRepo;
        private readonly IQualityTemplateQueryRepository _templateQueryRepo;
        private readonly IInventoryCategoryLookup _categoryLookup;
        private readonly IItemLookup _itemLookup;

        public CreateQualitySpecificationCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IQualitySpecificationQueryRepository queryRepo,
            IQualityTemplateQueryRepository templateQueryRepo,
            IInventoryCategoryLookup categoryLookup,
            IItemLookup itemLookup)
        {
            _queryRepo = queryRepo;
            _templateQueryRepo = templateQueryRepo;
            _categoryLookup = categoryLookup;
            _itemLookup = itemLookup;

            var maxLengthName = maxLengthProvider.GetMaxLength<Domain.Entities.QualitySpecification>("SpecificationName") ?? 100;
            var maxLengthDescription = maxLengthProvider.GetMaxLength<Domain.Entities.QualitySpecification>("Description") ?? 500;
            var maxLengthRemarks = maxLengthProvider.GetMaxLength<Domain.Entities.QualitySpecificationParameter>("Remarks") ?? 500;

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
                        RuleFor(x => x.SpecificationName)
                            .NotNull().WithMessage($"{nameof(CreateQualitySpecificationCommand.SpecificationName)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateQualitySpecificationCommand.SpecificationName)} {rule.Error}");

                        RuleFor(x => x.EffectiveFrom)
                            .NotEmpty().WithMessage($"{nameof(CreateQualitySpecificationCommand.EffectiveFrom)} {rule.Error}");

                        RuleFor(x => x.Parameters)
                            .NotNull().WithMessage($"Parameters {rule.Error}")
                            .Must(p => p != null && p.Count >= 1).WithMessage("At least one parameter row is required.");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.SpecificationName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(CreateQualitySpecificationCommand.SpecificationName)} {rule.Error} {maxLengthName} characters.");

                        RuleFor(x => x.Description)
                            .MaximumLength(maxLengthDescription)
                            .WithMessage($"{nameof(CreateQualitySpecificationCommand.Description)} {rule.Error} {maxLengthDescription} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Description));

                        RuleForEach(x => x.Parameters).ChildRules(p =>
                        {
                            p.RuleFor(d => d.Remarks)
                                .MaximumLength(maxLengthRemarks)
                                .WithMessage($"Remarks {rule.Error} {maxLengthRemarks} characters.")
                                .When(d => !string.IsNullOrWhiteSpace(d.Remarks));
                        }).When(x => x.Parameters != null && x.Parameters.Count > 0);
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.SpecificationName)
                            .MustAsync(async (name, ct) => !await _queryRepo.AlreadyExistsAsync(name!))
                            .WithMessage($"{nameof(CreateQualitySpecificationCommand.SpecificationName)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.SpecificationName));
                        break;

                    case "GreaterThan":
                        RuleFor(x => x.QualityTemplateId)
                            .GreaterThan(0).WithMessage($"QualityTemplateId {rule.Error}");

                        RuleFor(x => x.ApplicableLevelId)
                            .GreaterThan(0).WithMessage($"ApplicableLevelId {rule.Error}");

                        RuleForEach(x => x.Parameters).ChildRules(p =>
                        {
                            p.RuleFor(d => d.QualityParameterId)
                                .GreaterThan(0).WithMessage($"QualityParameterId {rule.Error}");

                            p.RuleFor(d => d.ValidationTypeId)
                                .GreaterThan(0).WithMessage($"ValidationTypeId {rule.Error}");
                        }).When(x => x.Parameters != null && x.Parameters.Count > 0);
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.QualityTemplateId)
                            .MustAsync(async (id, ct) =>
                            {
                                var tpl = await _templateQueryRepo.GetByIdAsync(id);
                                return tpl != null && tpl.IsActive;
                            })
                            .WithMessage($"QualityTemplateId {rule.Error}")
                            .When(x => x.QualityTemplateId > 0);

                        RuleFor(x => x.ApplicableLevelId)
                            .MustAsync(async (id, ct) => await _queryRepo.ApplicableLevelExistsAsync(id))
                            .WithMessage($"ApplicableLevelId {rule.Error}")
                            .When(x => x.ApplicableLevelId > 0);

                        RuleForEach(x => x.Parameters).ChildRules(p =>
                        {
                            p.RuleFor(d => d.ValidationTypeId)
                                .MustAsync(async (id, ct) => await _queryRepo.ValidationTypeExistsAsync(id))
                                .WithMessage($"ValidationTypeId {rule.Error}")
                                .When(d => d.ValidationTypeId > 0);

                            p.RuleFor(d => d.SeverityId)
                                .MustAsync(async (id, ct) => await _queryRepo.SeverityExistsAsync(id!.Value))
                                .WithMessage($"SeverityId {rule.Error}")
                                .When(d => d.SeverityId.HasValue && d.SeverityId.Value > 0);

                            p.RuleFor(d => d.FailureActionId)
                                .MustAsync(async (id, ct) => await _queryRepo.FailureActionExistsAsync(id!.Value))
                                .WithMessage($"FailureActionId {rule.Error}")
                                .When(d => d.FailureActionId.HasValue && d.FailureActionId.Value > 0);
                        }).When(x => x.Parameters != null && x.Parameters.Count > 0);
                        break;

                    case "DateCompare":
                        RuleFor(x => x.EffectiveTo)
                            .Must((cmd, to) => to == null || to.Value > cmd.EffectiveFrom)
                            .WithMessage($"EffectiveTo {rule.Error} EffectiveFrom.");
                        break;

                    default:
                        break;
                }
            }

            // ── Conditional Item/Category requirement based on ApplicableLevel ─────
            RuleFor(x => x)
                .MustAsync(async (cmd, ct) =>
                {
                    if (cmd.ApplicableLevelId <= 0) return true;
                    var levelCode = await _queryRepo.GetApplicableLevelCodeAsync(cmd.ApplicableLevelId);
                    if (string.IsNullOrWhiteSpace(levelCode)) return true;

                    if (levelCode == "ITEM CATEGORY")
                        return cmd.ItemCategoryId.HasValue && cmd.ItemCategoryId.Value > 0 && (!cmd.ItemId.HasValue || cmd.ItemId.Value == 0);
                    if (levelCode == "ITEM")
                        return cmd.ItemId.HasValue && cmd.ItemId.Value > 0 && (!cmd.ItemCategoryId.HasValue || cmd.ItemCategoryId.Value == 0);
                    return true;
                })
                .WithMessage("When Applicable Level is 'Item Category', ItemCategoryId is required and ItemId must be empty. When Applicable Level is 'Item', ItemId is required and ItemCategoryId must be empty.")
                .When(x => x.ApplicableLevelId > 0);

            // ── ItemCategoryId existence via cross-module lookup ───────────────────
            RuleFor(x => x.ItemCategoryId)
                .MustAsync(async (id, ct) =>
                {
                    var found = await _categoryLookup.GetCategoryByIdsAsync(new[] { id!.Value }, ct);
                    return found.Any();
                })
                .WithMessage("ItemCategoryId does not exist or is inactive.")
                .When(x => x.ItemCategoryId.HasValue && x.ItemCategoryId.Value > 0);

            // ── ItemId existence via cross-module lookup ───────────────────────────
            RuleFor(x => x.ItemId)
                .MustAsync(async (id, ct) =>
                {
                    var found = await _itemLookup.GetByIdsAsync(new[] { id!.Value }, ct);
                    return found.Any();
                })
                .WithMessage("ItemId does not exist or is inactive.")
                .When(x => x.ItemId.HasValue && x.ItemId.Value > 0);

            // ── No duplicate QualityParameterId in the parameter set ───────────────
            RuleFor(x => x.Parameters)
                .Must(list => list == null || list.Select(p => p.QualityParameterId).Distinct().Count() == list.Count)
                .WithMessage("Duplicate parameters within the same specification are not allowed.")
                .When(x => x.Parameters != null && x.Parameters.Count > 0);

            // ── Parameter set must match template's parameters EXACTLY ─────────────
            RuleFor(x => x)
                .MustAsync(async (cmd, ct) =>
                {
                    if (cmd.Parameters == null || cmd.Parameters.Count == 0 || cmd.QualityTemplateId <= 0)
                        return true;

                    var tpl = await _templateQueryRepo.GetByIdAsync(cmd.QualityTemplateId);
                    if (tpl?.Parameters == null) return false;

                    var templateIds = tpl.Parameters
                        .Where(p => p.IsActive)
                        .Select(p => p.QualityParameterId)
                        .ToHashSet();

                    var incomingIds = cmd.Parameters.Select(p => p.QualityParameterId).ToHashSet();

                    return templateIds.SetEquals(incomingIds);
                })
                .WithMessage("Parameters must match the selected template's parameters exactly — no additions or removals are allowed.")
                .When(x => x.Parameters != null && x.Parameters.Count > 0 && x.QualityTemplateId > 0);

            // ── Per-Validation-Type acceptance rules ───────────────────────────────
            RuleFor(x => x.Parameters)
                .MustAsync(async (parameters, ct) =>
                {
                    if (parameters == null || parameters.Count == 0) return true;

                    var validationTypeIds = parameters
                        .Where(p => p.ValidationTypeId > 0)
                        .Select(p => p.ValidationTypeId)
                        .Distinct()
                        .ToList();

                    if (validationTypeIds.Count == 0) return true;

                    var codeMap = await _queryRepo.GetValidationTypeCodesByIdsAsync(validationTypeIds);

                    foreach (var p in parameters)
                    {
                        if (!codeMap.TryGetValue(p.ValidationTypeId, out var code))
                            continue;

                        switch (code)
                        {
                            case "RNG":
                                if (!p.MinValue.HasValue || !p.MaxValue.HasValue) return false;
                                if (p.MinValue.Value > p.MaxValue.Value) return false;
                                if (!string.IsNullOrWhiteSpace(p.ExpectedValue)) return false;
                                if (p.AllowedValues != null && p.AllowedValues.Count > 0) return false;
                                break;

                            case "MIN":
                                if (!p.MinValue.HasValue) return false;
                                if (p.MaxValue.HasValue || !string.IsNullOrWhiteSpace(p.ExpectedValue)) return false;
                                if (p.AllowedValues != null && p.AllowedValues.Count > 0) return false;
                                break;

                            case "MAX":
                                if (!p.MaxValue.HasValue) return false;
                                if (p.MinValue.HasValue || !string.IsNullOrWhiteSpace(p.ExpectedValue)) return false;
                                if (p.AllowedValues != null && p.AllowedValues.Count > 0) return false;
                                break;

                            case "FIX":
                                if (string.IsNullOrWhiteSpace(p.ExpectedValue)) return false;
                                if (p.MinValue.HasValue || p.MaxValue.HasValue) return false;
                                if (p.AllowedValues != null && p.AllowedValues.Count > 0) return false;
                                break;

                            case "PFL":
                                if (p.MinValue.HasValue || p.MaxValue.HasValue) return false;
                                if (!string.IsNullOrWhiteSpace(p.ExpectedValue)) return false;
                                if (p.AllowedValues != null && p.AllowedValues.Count > 0) return false;
                                break;

                            case "LST":
                                if (p.AllowedValues == null || p.AllowedValues.Count == 0) return false;
                                var trimmed = p.AllowedValues.Select(v => v?.Trim() ?? string.Empty).ToList();
                                if (trimmed.Any(v => string.IsNullOrWhiteSpace(v))) return false;
                                if (trimmed.Any(v => v.Contains('|'))) return false;
                                if (trimmed.Distinct(StringComparer.OrdinalIgnoreCase).Count() != trimmed.Count) return false;
                                if (p.MinValue.HasValue || p.MaxValue.HasValue) return false;
                                if (!string.IsNullOrWhiteSpace(p.ExpectedValue)) return false;
                                break;
                        }
                    }
                    return true;
                })
                .WithMessage("Acceptance criteria do not match the selected Validation Type rules. " +
                             "Range requires Min<=Max; Min/Max require their respective value; Fixed requires ExpectedValue; " +
                             "Pass/Fail requires no extra fields; List Selection requires non-empty unique values with no '|' character.")
                .When(x => x.Parameters != null && x.Parameters.Count > 0);

            // ── Overlap detection — only one active spec per Item/Category in overlapping period ──
            RuleFor(x => x)
                .MustAsync(async (cmd, ct) =>
                {
                    if (cmd.ApplicableLevelId <= 0) return true;
                    if (!cmd.ItemCategoryId.HasValue && !cmd.ItemId.HasValue) return true;

                    return !await _queryRepo.HasOverlappingActiveSpecAsync(
                        cmd.ItemCategoryId, cmd.ItemId, cmd.EffectiveFrom, cmd.EffectiveTo);
                })
                .WithMessage("Another active Quality Specification already exists for the same Item or Item Category in an overlapping effective period.")
                .When(x => x.EffectiveFrom != default);
        }
    }
}
