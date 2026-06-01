using FluentValidation;
using QCManagement.Application.Common.Interfaces.IQualitySpecification;
using QCManagement.Application.QualitySpecification.Commands.UpdateQualitySpecification;
using QCManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace QCManagement.Presentation.Validation.QualitySpecification
{
    public class UpdateQualitySpecificationCommandValidator : AbstractValidator<UpdateQualitySpecificationCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IQualitySpecificationQueryRepository _queryRepo;

        public UpdateQualitySpecificationCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IQualitySpecificationQueryRepository queryRepo)
        {
            _queryRepo = queryRepo;

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
                            .NotNull().WithMessage($"{nameof(UpdateQualitySpecificationCommand.SpecificationName)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(UpdateQualitySpecificationCommand.SpecificationName)} {rule.Error}");

                        RuleFor(x => x.EffectiveFrom)
                            .NotEmpty().WithMessage($"{nameof(UpdateQualitySpecificationCommand.EffectiveFrom)} {rule.Error}");

                        RuleFor(x => x.Parameters)
                            .NotNull().WithMessage($"Parameters {rule.Error}")
                            .Must(p => p != null && p.Count >= 1).WithMessage("At least one parameter row is required.");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.SpecificationName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(UpdateQualitySpecificationCommand.SpecificationName)} {rule.Error} {maxLengthName} characters.");

                        RuleFor(x => x.Description)
                            .MaximumLength(maxLengthDescription)
                            .WithMessage($"{nameof(UpdateQualitySpecificationCommand.Description)} {rule.Error} {maxLengthDescription} characters.")
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
                            .MustAsync(async (cmd, name, ct) => !await _queryRepo.AlreadyExistsAsync(name!, cmd.Id))
                            .WithMessage($"{nameof(UpdateQualitySpecificationCommand.SpecificationName)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.SpecificationName));
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepo.NotFoundAsync(id))
                            .WithMessage($"QualitySpecification {rule.Error}");
                        break;

                    case "GreaterThan":
                        RuleFor(x => x.QcTypeId)
                            .GreaterThan(0).WithMessage($"QcTypeId {rule.Error}");

                        RuleForEach(x => x.Parameters).ChildRules(p =>
                        {
                            p.RuleFor(d => d.Id)
                                .GreaterThan(0).WithMessage($"Parameter Id {rule.Error}");

                            p.RuleFor(d => d.ValidationTypeId)
                                .GreaterThan(0).WithMessage($"ValidationTypeId {rule.Error}");
                        }).When(x => x.Parameters != null && x.Parameters.Count > 0);
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.QcTypeId)
                            .MustAsync(async (id, ct) => await _queryRepo.QcTypeExistsAsync(id))
                            .WithMessage($"QcTypeId {rule.Error}")
                            .When(x => x.QcTypeId > 0);

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

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateQualitySpecificationCommand.IsActive)} {rule.Error}");

                        RuleForEach(x => x.Parameters).ChildRules(p =>
                        {
                            p.RuleFor(d => d.IsActive)
                                .InclusiveBetween(0, 1)
                                .WithMessage($"Parameter IsActive {rule.Error}");
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

            // ── Incoming parameter Id set MUST match existing rows exactly ──────────
            RuleFor(x => x)
                .MustAsync(async (cmd, ct) =>
                {
                    if (cmd.Parameters == null || cmd.Parameters.Count == 0 || cmd.Id <= 0)
                        return true;

                    var existingIds = (await _queryRepo.GetExistingParameterRowIdsAsync(cmd.Id)).ToHashSet();
                    var incomingIds = cmd.Parameters.Select(p => p.Id).ToHashSet();

                    return existingIds.SetEquals(incomingIds);
                })
                .WithMessage("Parameter rows cannot be added or removed on update — each incoming Id must match an existing row.")
                .When(x => x.Id > 0 && x.Parameters != null && x.Parameters.Count > 0);

            // ── Per-Validation-Type acceptance rules (same as Create) ───────────────
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
                .WithMessage("Acceptance criteria do not match the selected Validation Type rules.")
                .When(x => x.Parameters != null && x.Parameters.Count > 0);

            // ── Overlap detection (excludes self; uses existing Item/Category from DB) ─
            RuleFor(x => x)
                .MustAsync(async (cmd, ct) =>
                {
                    if (cmd.Id <= 0) return true;
                    var (categoryId, itemId) = await _queryRepo.GetSpecificationItemContextAsync(cmd.Id);
                    if (!categoryId.HasValue && !itemId.HasValue) return true;

                    return !await _queryRepo.HasOverlappingActiveSpecAsync(
                        categoryId, itemId, cmd.EffectiveFrom, cmd.EffectiveTo, excludeSpecId: cmd.Id);
                })
                .WithMessage("Another active Quality Specification already exists for the same Item or Item Category in an overlapping effective period.")
                .When(x => x.Id > 0 && x.IsActive == 1);
        }
    }
}
