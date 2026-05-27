using FluentValidation;
using PurchaseManagement.Application.Common.Interfaces.IVendorEvaluationCriteria;
using PurchaseManagement.Application.VendorEvaluationCriteria.Commands.CreateVendorEvaluationCriteria;
using PurchaseManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace PurchaseManagement.Presentation.Validation.VendorEvaluationCriteria
{
    public class CreateVendorEvaluationCriteriaCommandValidator : AbstractValidator<CreateVendorEvaluationCriteriaCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IVendorEvaluationCriteriaQueryRepository _queryRepo;

        public CreateVendorEvaluationCriteriaCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IVendorEvaluationCriteriaQueryRepository queryRepo)
        {
            _queryRepo = queryRepo;

            var maxLengthCode = maxLengthProvider.GetMaxLength<Domain.Entities.VendorEvaluation.VendorEvaluationCriteria>("CriteriaCode") ?? 20;
            var maxLengthName = maxLengthProvider.GetMaxLength<Domain.Entities.VendorEvaluation.VendorEvaluationCriteria>("CriteriaName") ?? 100;
            var maxLengthDesc = maxLengthProvider.GetMaxLength<Domain.Entities.VendorEvaluation.VendorEvaluationCriteria>("Description") ?? 500;

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
                        RuleFor(x => x.CriteriaCode)
                            .NotNull().WithMessage($"{nameof(CreateVendorEvaluationCriteriaCommand.CriteriaCode)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateVendorEvaluationCriteriaCommand.CriteriaCode)} {rule.Error}");

                        RuleFor(x => x.CriteriaName)
                            .NotNull().WithMessage($"{nameof(CreateVendorEvaluationCriteriaCommand.CriteriaName)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateVendorEvaluationCriteriaCommand.CriteriaName)} {rule.Error}");
                        break;

                    case "Alphanumeric":
                        RuleFor(x => x.CriteriaCode)
                            .Matches(rule.Pattern)
                            .WithMessage($"{nameof(CreateVendorEvaluationCriteriaCommand.CriteriaCode)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.CriteriaCode));
                        break;

                    case "MaxLength":
                        RuleFor(x => x.CriteriaCode)
                            .MaximumLength(maxLengthCode)
                            .WithMessage($"{nameof(CreateVendorEvaluationCriteriaCommand.CriteriaCode)} {rule.Error} {maxLengthCode} characters.");

                        RuleFor(x => x.CriteriaName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(CreateVendorEvaluationCriteriaCommand.CriteriaName)} {rule.Error} {maxLengthName} characters.");

                        RuleFor(x => x.Description)
                            .MaximumLength(maxLengthDesc)
                            .WithMessage($"{nameof(CreateVendorEvaluationCriteriaCommand.Description)} {rule.Error} {maxLengthDesc} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Description));
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.ScoringMethodId)
                            .MustAsync(async (id, ct) => await _queryRepo.ScoringMethodExistsAsync(id))
                            .WithMessage($"{nameof(CreateVendorEvaluationCriteriaCommand.ScoringMethodId)} {rule.Error}")
                            .When(x => x.ScoringMethodId > 0);

                        RuleFor(x => x.RatingImpactId)
                            .MustAsync(async (id, ct) => await _queryRepo.RatingImpactExistsAsync(id))
                            .WithMessage($"{nameof(CreateVendorEvaluationCriteriaCommand.RatingImpactId)} {rule.Error}")
                            .When(x => x.RatingImpactId > 0);
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.CriteriaCode)
                            .MustAsync(async (code, ct) => !await _queryRepo.AlreadyExistsAsync(code!))
                            .WithMessage($"{nameof(CreateVendorEvaluationCriteriaCommand.CriteriaCode)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.CriteriaCode));
                        break;

                    case "GreaterThanOrEqualToZero":
                        RuleFor(x => x.WeightagePercent)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(CreateVendorEvaluationCriteriaCommand.WeightagePercent)} {rule.Error}");

                        RuleFor(x => x.MinimumScore)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(CreateVendorEvaluationCriteriaCommand.MinimumScore)} {rule.Error}");

                        RuleFor(x => x.SortOrder)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(CreateVendorEvaluationCriteriaCommand.SortOrder)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
