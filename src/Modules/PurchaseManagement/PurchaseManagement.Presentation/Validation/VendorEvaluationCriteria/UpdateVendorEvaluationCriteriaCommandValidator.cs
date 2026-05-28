using FluentValidation;
using PurchaseManagement.Application.Common.Interfaces.IVendorEvaluationCriteria;
using PurchaseManagement.Application.VendorEvaluationCriteria.Commands.UpdateVendorEvaluationCriteria;
using PurchaseManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace PurchaseManagement.Presentation.Validation.VendorEvaluationCriteria
{
    public class UpdateVendorEvaluationCriteriaCommandValidator : AbstractValidator<UpdateVendorEvaluationCriteriaCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IVendorEvaluationCriteriaQueryRepository _queryRepo;

        public UpdateVendorEvaluationCriteriaCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IVendorEvaluationCriteriaQueryRepository queryRepo)
        {
            _queryRepo = queryRepo;

            var maxLengthName = maxLengthProvider.GetMaxLength<Domain.Entities.VendorEvaluation.VendorEvaluationCriteria>("CriteriaName") ?? 100;
            var maxLengthDesc = maxLengthProvider.GetMaxLength<Domain.Entities.VendorEvaluation.VendorEvaluationCriteria>("Description") ?? 500;
            var maxLengthCalcType = maxLengthProvider.GetMaxLength<Domain.Entities.VendorEvaluation.VendorEvaluationCriteria>("CalculationType") ?? 30;

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
                        RuleFor(x => x.CriteriaName)
                            .NotNull().WithMessage($"{nameof(UpdateVendorEvaluationCriteriaCommand.CriteriaName)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(UpdateVendorEvaluationCriteriaCommand.CriteriaName)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.CriteriaName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(UpdateVendorEvaluationCriteriaCommand.CriteriaName)} {rule.Error} {maxLengthName} characters.");

                        RuleFor(x => x.Description)
                            .MaximumLength(maxLengthDesc)
                            .WithMessage($"{nameof(UpdateVendorEvaluationCriteriaCommand.Description)} {rule.Error} {maxLengthDesc} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Description));

                        RuleFor(x => x.CalculationType)
                            .MaximumLength(maxLengthCalcType)
                            .WithMessage($"{nameof(UpdateVendorEvaluationCriteriaCommand.CalculationType)} {rule.Error} {maxLengthCalcType} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.CalculationType));
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepo.NotFoundAsync(id))
                            .WithMessage($"VendorEvaluationCriteria {rule.Error}");
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.ScoringMethodId)
                            .MustAsync(async (id, ct) => await _queryRepo.ScoringMethodExistsAsync(id))
                            .WithMessage($"{nameof(UpdateVendorEvaluationCriteriaCommand.ScoringMethodId)} {rule.Error}")
                            .When(x => x.ScoringMethodId > 0);

                        RuleFor(x => x.RatingImpactId)
                            .MustAsync(async (id, ct) => await _queryRepo.RatingImpactExistsAsync(id))
                            .WithMessage($"{nameof(UpdateVendorEvaluationCriteriaCommand.RatingImpactId)} {rule.Error}")
                            .When(x => x.RatingImpactId > 0);
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateVendorEvaluationCriteriaCommand.IsActive)} {rule.Error}");
                        break;

                    case "GreaterThanOrEqualToZero":
                        RuleFor(x => x.WeightagePercent)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(UpdateVendorEvaluationCriteriaCommand.WeightagePercent)} {rule.Error}");

                        RuleFor(x => x.MinimumScore)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(UpdateVendorEvaluationCriteriaCommand.MinimumScore)} {rule.Error}");

                        RuleFor(x => x.SortOrder)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(UpdateVendorEvaluationCriteriaCommand.SortOrder)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
