using FluentValidation;
using PurchaseManagement.Application.Common.Interfaces.IVendorRatingGrade;
using PurchaseManagement.Application.VendorRatingGrade.Commands.UpdateVendorRatingGrade;
using PurchaseManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace PurchaseManagement.Presentation.Validation.VendorRatingGrade
{
    public class UpdateVendorRatingGradeCommandValidator : AbstractValidator<UpdateVendorRatingGradeCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IVendorRatingGradeQueryRepository _queryRepo;

        public UpdateVendorRatingGradeCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IVendorRatingGradeQueryRepository queryRepo)
        {
            _queryRepo = queryRepo;

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
                        RuleFor(x => x.GradeName)
                            .NotNull().WithMessage($"{nameof(UpdateVendorRatingGradeCommand.GradeName)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(UpdateVendorRatingGradeCommand.GradeName)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.GradeName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(UpdateVendorRatingGradeCommand.GradeName)} {rule.Error} {maxLengthName} characters.");

                        RuleFor(x => x.ActionDescription)
                            .MaximumLength(maxLengthDesc)
                            .WithMessage($"{nameof(UpdateVendorRatingGradeCommand.ActionDescription)} {rule.Error} {maxLengthDesc} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.ActionDescription));
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepo.NotFoundAsync(id))
                            .WithMessage($"VendorRatingGrade {rule.Error}");
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.ActionTypeId)
                            .MustAsync(async (id, ct) => await _queryRepo.ActionTypeExistsAsync(id!.Value))
                            .WithMessage($"{nameof(UpdateVendorRatingGradeCommand.ActionTypeId)} {rule.Error}")
                            .When(x => x.ActionTypeId.HasValue && x.ActionTypeId.Value > 0);
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateVendorRatingGradeCommand.IsActive)} {rule.Error}");
                        break;

                    case "GreaterThanOrEqualToZero":
                        RuleFor(x => x.MinScore)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(UpdateVendorRatingGradeCommand.MinScore)} {rule.Error}");

                        RuleFor(x => x.MaxScore)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(UpdateVendorRatingGradeCommand.MaxScore)} {rule.Error}");

                        RuleFor(x => x.SortOrder)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(UpdateVendorRatingGradeCommand.SortOrder)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
