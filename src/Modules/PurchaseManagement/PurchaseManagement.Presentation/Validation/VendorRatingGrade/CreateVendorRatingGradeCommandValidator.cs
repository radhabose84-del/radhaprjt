using FluentValidation;
using PurchaseManagement.Application.Common.Interfaces.IVendorRatingGrade;
using PurchaseManagement.Application.VendorRatingGrade.Commands.CreateVendorRatingGrade;
using PurchaseManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace PurchaseManagement.Presentation.Validation.VendorRatingGrade
{
    public class CreateVendorRatingGradeCommandValidator : AbstractValidator<CreateVendorRatingGradeCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IVendorRatingGradeQueryRepository _queryRepo;

        public CreateVendorRatingGradeCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IVendorRatingGradeQueryRepository queryRepo)
        {
            _queryRepo = queryRepo;

            var maxLengthCode = maxLengthProvider.GetMaxLength<Domain.Entities.VendorEvaluation.VendorRatingGrade>("GradeCode") ?? 20;
            var maxLengthName = maxLengthProvider.GetMaxLength<Domain.Entities.VendorEvaluation.VendorRatingGrade>("GradeName") ?? 100;
            var maxLengthDesc = maxLengthProvider.GetMaxLength<Domain.Entities.VendorEvaluation.VendorRatingGrade>("ActionDescription") ?? 500;

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
                        RuleFor(x => x.GradeCode)
                            .NotNull().WithMessage($"{nameof(CreateVendorRatingGradeCommand.GradeCode)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateVendorRatingGradeCommand.GradeCode)} {rule.Error}");

                        RuleFor(x => x.GradeName)
                            .NotNull().WithMessage($"{nameof(CreateVendorRatingGradeCommand.GradeName)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateVendorRatingGradeCommand.GradeName)} {rule.Error}");
                        break;

                    case "Alphanumeric":
                        RuleFor(x => x.GradeCode)
                            .Matches(rule.Pattern)
                            .WithMessage($"{nameof(CreateVendorRatingGradeCommand.GradeCode)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.GradeCode));
                        break;

                    case "MaxLength":
                        RuleFor(x => x.GradeCode)
                            .MaximumLength(maxLengthCode)
                            .WithMessage($"{nameof(CreateVendorRatingGradeCommand.GradeCode)} {rule.Error} {maxLengthCode} characters.");

                        RuleFor(x => x.GradeName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(CreateVendorRatingGradeCommand.GradeName)} {rule.Error} {maxLengthName} characters.");

                        RuleFor(x => x.ActionDescription)
                            .MaximumLength(maxLengthDesc)
                            .WithMessage($"{nameof(CreateVendorRatingGradeCommand.ActionDescription)} {rule.Error} {maxLengthDesc} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.ActionDescription));
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.ActionTypeId)
                            .MustAsync(async (id, ct) => await _queryRepo.ActionTypeExistsAsync(id!.Value))
                            .WithMessage($"{nameof(CreateVendorRatingGradeCommand.ActionTypeId)} {rule.Error}")
                            .When(x => x.ActionTypeId.HasValue && x.ActionTypeId.Value > 0);
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.GradeCode)
                            .MustAsync(async (code, ct) => !await _queryRepo.AlreadyExistsAsync(code!))
                            .WithMessage($"{nameof(CreateVendorRatingGradeCommand.GradeCode)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.GradeCode));
                        break;

                    case "GreaterThanOrEqualToZero":
                        RuleFor(x => x.MinScore)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(CreateVendorRatingGradeCommand.MinScore)} {rule.Error}");

                        RuleFor(x => x.MaxScore)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(CreateVendorRatingGradeCommand.MaxScore)} {rule.Error}");

                        RuleFor(x => x.SortOrder)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(CreateVendorRatingGradeCommand.SortOrder)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
