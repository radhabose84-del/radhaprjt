using FluentValidation;
using PurchaseManagement.Application.Common.Interfaces.IVendorEvaluationHeader;
using PurchaseManagement.Application.VendorEvaluationHeader.Commands.CreateVendorEvaluationHeader;
using PurchaseManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace PurchaseManagement.Presentation.Validation.VendorEvaluationHeader
{
    public class CreateVendorEvaluationHeaderCommandValidator : AbstractValidator<CreateVendorEvaluationHeaderCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IVendorEvaluationHeaderQueryRepository _queryRepo;

        public CreateVendorEvaluationHeaderCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IVendorEvaluationHeaderQueryRepository queryRepo)
        {
            _queryRepo = queryRepo;

            var maxLengthCode = maxLengthProvider.GetMaxLength<Domain.Entities.VendorEvaluation.VendorEvaluationHeader>("EvaluationCode") ?? 20;
            var maxLengthRemarks = maxLengthProvider.GetMaxLength<Domain.Entities.VendorEvaluation.VendorEvaluationHeader>("Remarks") ?? 500;

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
                        RuleFor(x => x.EvaluationCode)
                            .NotNull().WithMessage($"{nameof(CreateVendorEvaluationHeaderCommand.EvaluationCode)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateVendorEvaluationHeaderCommand.EvaluationCode)} {rule.Error}");
                        break;

                    case "Alphanumeric":
                        RuleFor(x => x.EvaluationCode)
                            .Matches(rule.Pattern)
                            .WithMessage($"{nameof(CreateVendorEvaluationHeaderCommand.EvaluationCode)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.EvaluationCode));
                        break;

                    case "MaxLength":
                        RuleFor(x => x.EvaluationCode)
                            .MaximumLength(maxLengthCode)
                            .WithMessage($"{nameof(CreateVendorEvaluationHeaderCommand.EvaluationCode)} {rule.Error} {maxLengthCode} characters.");

                        RuleFor(x => x.Remarks)
                            .MaximumLength(maxLengthRemarks)
                            .WithMessage($"{nameof(CreateVendorEvaluationHeaderCommand.Remarks)} {rule.Error} {maxLengthRemarks} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Remarks));
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.VendorId)
                            .MustAsync(async (id, ct) => await _queryRepo.VendorExistsAsync(id))
                            .WithMessage($"{nameof(CreateVendorEvaluationHeaderCommand.VendorId)} {rule.Error}")
                            .When(x => x.VendorId > 0);

                        RuleFor(x => x.GradeId)
                            .MustAsync(async (id, ct) => await _queryRepo.GradeExistsAsync(id!.Value))
                            .WithMessage($"{nameof(CreateVendorEvaluationHeaderCommand.GradeId)} {rule.Error}")
                            .When(x => x.GradeId.HasValue && x.GradeId.Value > 0);

                        RuleFor(x => x.StatusId)
                            .MustAsync(async (id, ct) => await _queryRepo.StatusExistsAsync(id))
                            .WithMessage($"{nameof(CreateVendorEvaluationHeaderCommand.StatusId)} {rule.Error}")
                            .When(x => x.StatusId > 0);
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.EvaluationCode)
                            .MustAsync(async (code, ct) => !await _queryRepo.AlreadyExistsAsync(code!))
                            .WithMessage($"{nameof(CreateVendorEvaluationHeaderCommand.EvaluationCode)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.EvaluationCode));

                        RuleFor(x => x)
                            .MustAsync(async (cmd, ct) => !await _queryRepo.CompositeKeyExistsAsync(cmd.VendorId, cmd.EvaluationMonth, cmd.EvaluationYear))
                            .WithMessage("An evaluation for this Vendor, Month, and Year already exists.")
                            .When(x => x.VendorId > 0 && x.EvaluationMonth > 0 && x.EvaluationYear > 0);
                        break;

                    case "GreaterThan":
                        RuleFor(x => x.VendorId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateVendorEvaluationHeaderCommand.VendorId)} {rule.Error}");

                        RuleFor(x => x.StatusId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateVendorEvaluationHeaderCommand.StatusId)} {rule.Error}");

                        RuleFor(x => x.EvaluationMonth)
                            .InclusiveBetween(1, 12)
                            .WithMessage($"{nameof(CreateVendorEvaluationHeaderCommand.EvaluationMonth)} must be between 1 and 12.");

                        RuleFor(x => x.EvaluationYear)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateVendorEvaluationHeaderCommand.EvaluationYear)} {rule.Error}");

                        When(x => x.Details != null && x.Details.Count > 0, () =>
                        {
                            RuleForEach(x => x.Details).ChildRules(detail =>
                            {
                                detail.RuleFor(d => d.CriteriaId)
                                    .GreaterThan(0)
                                    .WithMessage($"CriteriaId {rule.Error}");

                                detail.RuleFor(d => d.Score)
                                    .GreaterThanOrEqualTo(0)
                                    .WithMessage("Score must be zero or positive.");

                                detail.RuleFor(d => d.WeightagePercent)
                                    .GreaterThanOrEqualTo(0)
                                    .WithMessage("WeightagePercent must be zero or positive.");

                                detail.RuleFor(d => d.WeightedScore)
                                    .GreaterThanOrEqualTo(0)
                                    .WithMessage("WeightedScore must be zero or positive.");
                            });
                        });
                        break;

                    case "GreaterThanOrEqualToZero":
                        RuleFor(x => x.TotalWeightedScore)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(CreateVendorEvaluationHeaderCommand.TotalWeightedScore)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
