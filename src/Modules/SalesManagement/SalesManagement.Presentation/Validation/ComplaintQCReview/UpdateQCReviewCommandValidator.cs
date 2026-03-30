using FluentValidation;
using SalesManagement.Application.Common.Interfaces.IComplaintQCReview;
using SalesManagement.Application.ComplaintQCReview.Commands.UpdateQCReview;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.ComplaintQCReview
{
    public class UpdateQCReviewCommandValidator : AbstractValidator<UpdateQCReviewCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IComplaintQCReviewQueryRepository _queryRepository;

        public UpdateQCReviewCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IComplaintQCReviewQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            var maxLengthComments = maxLengthProvider.GetMaxLength<Domain.Entities.ComplaintQCReview>("Comments") ?? 1000;

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
                        RuleFor(x => x.PhysicalVerificationId)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateQCReviewCommand.PhysicalVerificationId)} {rule.Error}");

                        RuleFor(x => x.Comments)
                            .NotNull()
                            .WithMessage($"{nameof(UpdateQCReviewCommand.Comments)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateQCReviewCommand.Comments)} {rule.Error}");

                        RuleFor(x => x.ExpectedResolutionDate)
                            .NotNull()
                            .WithMessage($"{nameof(UpdateQCReviewCommand.ExpectedResolutionDate)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"QC Review {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.Comments)
                            .MaximumLength(maxLengthComments)
                            .WithMessage($"{nameof(UpdateQCReviewCommand.Comments)} {rule.Error} {maxLengthComments} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Comments));
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.PhysicalVerificationId)
                            .MustAsync(async (id, ct) => await _queryRepository.MiscMasterExistsAsync(id))
                            .WithMessage($"{nameof(UpdateQCReviewCommand.PhysicalVerificationId)} {rule.Error}")
                            .When(x => x.PhysicalVerificationId > 0);

                        RuleFor(x => x.ComplaintStatusId)
                            .MustAsync(async (id, ct) => await _queryRepository.MiscMasterExistsAsync(id!.Value))
                            .WithMessage($"{nameof(UpdateQCReviewCommand.ComplaintStatusId)} {rule.Error}")
                            .When(x => x.ComplaintStatusId.HasValue && x.ComplaintStatusId.Value > 0);

                        RuleFor(x => x.SeverityId)
                            .MustAsync(async (id, ct) => await _queryRepository.MiscMasterExistsAsync(id!.Value))
                            .WithMessage($"{nameof(UpdateQCReviewCommand.SeverityId)} {rule.Error}")
                            .When(x => x.SeverityId.HasValue && x.SeverityId.Value > 0);

                        RuleFor(x => x.CompensationStructureId)
                            .MustAsync(async (id, ct) => await _queryRepository.MiscMasterExistsAsync(id!.Value))
                            .WithMessage($"{nameof(UpdateQCReviewCommand.CompensationStructureId)} {rule.Error}")
                            .When(x => x.CompensationStructureId.HasValue && x.CompensationStructureId.Value > 0);

                        RuleFor(x => x.LabResponsiblePersonId)
                            .MustAsync(async (id, ct) => await _queryRepository.UserExistsAsync(id!.Value))
                            .WithMessage($"{nameof(UpdateQCReviewCommand.LabResponsiblePersonId)} {rule.Error}")
                            .When(x => x.LabResponsiblePersonId.HasValue && x.LabResponsiblePersonId.Value > 0);
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateQCReviewCommand.IsActive)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }

            // Business rule: Lab person mandatory if LabVerificationRequired = Yes
            RuleFor(x => x.LabResponsiblePersonId)
                .NotNull()
                .WithMessage("Lab Responsible Person is required when Lab Verification is required.")
                .GreaterThan(0)
                .WithMessage("Lab Responsible Person is required when Lab Verification is required.")
                .When(x => x.LabVerificationRequired);

            // Business rule: If Accepted → at least one assignment required
            RuleFor(x => x.Assignments)
                .Must(a => a != null && a.Count > 0)
                .WithMessage("At least one responsible person must be assigned when complaint is accepted.")
                .When(x => x.ComplaintStatusId.HasValue && x.ComplaintStatusId.Value > 0);

            // Business rule: If Accepted → CompensationStructure mandatory
            RuleFor(x => x.CompensationStructureId)
                .NotNull()
                .WithMessage("Compensation Structure is required when complaint is accepted.")
                .GreaterThan(0)
                .WithMessage("Compensation Structure is required when complaint is accepted.")
                .When(x => x.ComplaintStatusId.HasValue && x.ComplaintStatusId.Value > 0);

            // Validate assignment details
            RuleForEach(x => x.Assignments)
                .ChildRules(assignment =>
                {
                    assignment.RuleFor(a => a.RoleId)
                        .GreaterThan(0)
                        .WithMessage("Assignment Role is required.");

                    assignment.RuleFor(a => a.ResponsiblePersonId)
                        .GreaterThan(0)
                        .WithMessage("Responsible Person is required.");
                })
                .When(x => x.Assignments != null && x.Assignments.Count > 0);
        }
    }
}
