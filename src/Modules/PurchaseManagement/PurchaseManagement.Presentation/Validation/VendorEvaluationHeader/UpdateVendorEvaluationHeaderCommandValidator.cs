using FluentValidation;
using PurchaseManagement.Application.Common.Interfaces.IVendorEvaluationHeader;
using PurchaseManagement.Application.VendorEvaluationHeader.Commands.UpdateVendorEvaluationHeader;
using PurchaseManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace PurchaseManagement.Presentation.Validation.VendorEvaluationHeader
{
    public class UpdateVendorEvaluationHeaderCommandValidator : AbstractValidator<UpdateVendorEvaluationHeaderCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IVendorEvaluationHeaderQueryRepository _queryRepo;

        public UpdateVendorEvaluationHeaderCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IVendorEvaluationHeaderQueryRepository queryRepo)
        {
            _queryRepo = queryRepo;

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
                    case "MaxLength":
                        RuleFor(x => x.Remarks)
                            .MaximumLength(maxLengthRemarks)
                            .WithMessage($"{nameof(UpdateVendorEvaluationHeaderCommand.Remarks)} {rule.Error} {maxLengthRemarks} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Remarks));
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepo.NotFoundAsync(id))
                            .WithMessage($"VendorEvaluationHeader {rule.Error}");
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.VendorId)
                            .MustAsync(async (id, ct) => await _queryRepo.VendorExistsAsync(id))
                            .WithMessage($"{nameof(UpdateVendorEvaluationHeaderCommand.VendorId)} {rule.Error}")
                            .When(x => x.VendorId > 0);

                        RuleFor(x => x.GradeId)
                            .MustAsync(async (id, ct) => await _queryRepo.GradeExistsAsync(id!.Value))
                            .WithMessage($"{nameof(UpdateVendorEvaluationHeaderCommand.GradeId)} {rule.Error}")
                            .When(x => x.GradeId.HasValue && x.GradeId.Value > 0);
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x)
                            .MustAsync(async (cmd, ct) => !await _queryRepo.CompositeKeyExistsAsync(cmd.VendorId, cmd.EvaluationMonth, cmd.EvaluationYear, cmd.Id))
                            .WithMessage("An evaluation for this Vendor, Month, and Year already exists.")
                            .When(x => x.VendorId > 0 && x.EvaluationMonth > 0 && x.EvaluationYear > 0);
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateVendorEvaluationHeaderCommand.IsActive)} {rule.Error}");
                        break;

                    case "GreaterThan":
                        RuleFor(x => x.VendorId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateVendorEvaluationHeaderCommand.VendorId)} must be greater than zero.");

                        RuleFor(x => x.EvaluationMonth)
                            .InclusiveBetween(1, 12)
                            .WithMessage($"{nameof(UpdateVendorEvaluationHeaderCommand.EvaluationMonth)} must be between 1 and 12.");

                        RuleFor(x => x.EvaluationYear)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateVendorEvaluationHeaderCommand.EvaluationYear)} must be greater than zero.");

                        When(x => x.Details != null && x.Details.Count > 0, () =>
                        {
                            RuleForEach(x => x.Details).ChildRules(detail =>
                            {
                                detail.RuleFor(d => d.CriteriaId)
                                    .GreaterThan(0)
                                    .WithMessage("CriteriaId must be greater than zero.");

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
                            .WithMessage($"{nameof(UpdateVendorEvaluationHeaderCommand.TotalWeightedScore)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
