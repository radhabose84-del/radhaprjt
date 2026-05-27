using Contracts.Interfaces.Lookups.Inventory;
using FluentValidation;
using QCManagement.Application.Common.Interfaces.IQualityTemplate;
using QCManagement.Application.QualityTemplate.Commands.UpdateQualityTemplate;
using QCManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace QCManagement.Presentation.Validation.QualityTemplate
{
    public class UpdateQualityTemplateCommandValidator : AbstractValidator<UpdateQualityTemplateCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IQualityTemplateQueryRepository _queryRepo;
        private readonly IUOMLookup _uomLookup;

        public UpdateQualityTemplateCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IQualityTemplateQueryRepository queryRepo,
            IUOMLookup uomLookup)
        {
            _queryRepo = queryRepo;
            _uomLookup = uomLookup;

            var maxLengthName = maxLengthProvider.GetMaxLength<Domain.Entities.QualityTemplate>("TemplateName") ?? 100;
            var maxLengthDescription = maxLengthProvider.GetMaxLength<Domain.Entities.QualityTemplate>("Description") ?? 500;
            var maxLengthRemarks = maxLengthProvider.GetMaxLength<Domain.Entities.QualityTemplateParameter>("Remarks") ?? 500;

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
                        RuleFor(x => x.TemplateName)
                            .NotNull()
                            .WithMessage($"{nameof(UpdateQualityTemplateCommand.TemplateName)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateQualityTemplateCommand.TemplateName)} {rule.Error}");

                        RuleFor(x => x.Parameters)
                            .NotNull()
                            .WithMessage($"Parameters {rule.Error}")
                            .Must(p => p != null && p.Count >= 1)
                            .WithMessage("At least one parameter row is required.");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.TemplateName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(UpdateQualityTemplateCommand.TemplateName)} {rule.Error} {maxLengthName} characters.");

                        RuleFor(x => x.Description)
                            .MaximumLength(maxLengthDescription)
                            .WithMessage($"{nameof(UpdateQualityTemplateCommand.Description)} {rule.Error} {maxLengthDescription} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Description));

                        RuleForEach(x => x.Parameters).ChildRules(p =>
                        {
                            p.RuleFor(d => d.Remarks)
                                .MaximumLength(maxLengthRemarks)
                                .WithMessage($"Remarks {rule.Error} {maxLengthRemarks} characters.")
                                .When(d => !string.IsNullOrWhiteSpace(d.Remarks));
                        }).When(x => x.Parameters != null && x.Parameters.Count > 0);
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepo.NotFoundAsync(id))
                            .WithMessage($"Quality Template {rule.Error}");
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.TemplateName)
                            .MustAsync(async (cmd, name, ct) => !await _queryRepo.AlreadyExistsAsync(name!, cmd.Id))
                            .WithMessage($"{nameof(UpdateQualityTemplateCommand.TemplateName)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.TemplateName));
                        break;

                    case "FKColumnDelete":
                        RuleForEach(x => x.Parameters).ChildRules(p =>
                        {
                            p.RuleFor(d => d.QualityParameterId)
                                .MustAsync(async (id, ct) => await _queryRepo.QualityParameterExistsAndActiveAsync(id))
                                .WithMessage($"QualityParameterId {rule.Error}")
                                .When(d => d.QualityParameterId > 0);

                            p.RuleFor(d => d.InspectionMethodId)
                                .MustAsync(async (id, ct) => await _queryRepo.InspectionMethodExistsAsync(id!.Value))
                                .WithMessage($"InspectionMethodId {rule.Error}")
                                .When(d => d.InspectionMethodId.HasValue && d.InspectionMethodId.Value > 0);
                        }).When(x => x.Parameters != null && x.Parameters.Count > 0);
                        break;

                    case "GreaterThan":
                        RuleForEach(x => x.Parameters).ChildRules(p =>
                        {
                            p.RuleFor(d => d.QualityParameterId)
                                .GreaterThan(0)
                                .WithMessage($"QualityParameterId {rule.Error}");

                            p.RuleFor(d => d.SequenceNo)
                                .GreaterThan(0)
                                .WithMessage($"SequenceNo {rule.Error}");

                            p.RuleFor(d => d.SampleSize)
                                .GreaterThan(0)
                                .WithMessage($"SampleSize {rule.Error}")
                                .When(d => d.SampleSize.HasValue);
                        }).When(x => x.Parameters != null && x.Parameters.Count > 0);
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateQualityTemplateCommand.IsActive)} {rule.Error}");

                        RuleForEach(x => x.Parameters).ChildRules(p =>
                        {
                            p.RuleFor(d => d.IsActive)
                                .InclusiveBetween(0, 1)
                                .WithMessage($"Parameters.IsActive {rule.Error}");
                        }).When(x => x.Parameters != null && x.Parameters.Count > 0);
                        break;

                    default:
                        break;
                }
            }

            // ── Business rules — outside switch ───────────────────────────────

            RuleFor(x => x.Parameters)
                .Must(list => list == null || list.Select(p => p.QualityParameterId).Distinct().Count() == list.Count)
                .WithMessage("Duplicate parameters within the same template are not allowed.")
                .When(x => x.Parameters != null && x.Parameters.Count > 0);

            RuleForEach(x => x.Parameters).ChildRules(p =>
            {
                p.RuleFor(d => d.SampleUomId)
                    .NotNull().WithMessage("Sample UOM is required when Sample Size is provided.")
                    .GreaterThan(0).WithMessage("Sample UOM is required when Sample Size is provided.")
                    .When(d => d.SampleSize.HasValue && d.SampleSize.Value > 0);
            }).When(x => x.Parameters != null && x.Parameters.Count > 0);

            RuleFor(x => x.Parameters)
                .MustAsync(async (parameters, ct) =>
                {
                    if (parameters == null || parameters.Count == 0)
                        return true;

                    var uomIds = parameters
                        .Where(p => p.SampleUomId.HasValue && p.SampleUomId.Value > 0)
                        .Select(p => p.SampleUomId!.Value)
                        .Distinct()
                        .ToList();

                    if (uomIds.Count == 0)
                        return true;

                    var found = await _uomLookup.GetByIdsAsync(uomIds, ct);
                    var foundIds = found.Select(u => u.Id).ToHashSet();
                    return uomIds.All(id => foundIds.Contains(id));
                })
                .WithMessage("One or more Sample UOM values are invalid or inactive.")
                .When(x => x.Parameters != null && x.Parameters.Any(p => p.SampleUomId.HasValue && p.SampleUomId.Value > 0));
        }
    }
}
