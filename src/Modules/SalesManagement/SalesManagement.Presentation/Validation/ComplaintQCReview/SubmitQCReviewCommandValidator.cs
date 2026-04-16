using FluentValidation;
using SalesManagement.Application.Common.Interfaces.IComplaintQCReview;
using SalesManagement.Application.ComplaintQCReview.Commands.SubmitQCReview;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.ComplaintQCReview
{
    public class SubmitQCReviewCommandValidator : AbstractValidator<SubmitQCReviewCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IComplaintQCReviewQueryRepository _queryRepository;

        public SubmitQCReviewCommandValidator(
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
                        RuleFor(x => x.ComplaintHeaderId)
                            .NotEmpty()
                            .WithMessage($"{nameof(SubmitQCReviewCommand.ComplaintHeaderId)} {rule.Error}");

                        RuleFor(x => x.PhysicalVerificationId)
                            .NotEmpty()
                            .WithMessage($"{nameof(SubmitQCReviewCommand.PhysicalVerificationId)} {rule.Error}");

                        RuleFor(x => x.Comments)
                            .NotNull()
                            .WithMessage($"{nameof(SubmitQCReviewCommand.Comments)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(SubmitQCReviewCommand.Comments)} {rule.Error}");

                        RuleFor(x => x.ExpectedResolutionDate)
                            .NotNull()
                            .WithMessage($"{nameof(SubmitQCReviewCommand.ExpectedResolutionDate)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.Comments)
                            .MaximumLength(maxLengthComments)
                            .WithMessage($"{nameof(SubmitQCReviewCommand.Comments)} {rule.Error} {maxLengthComments} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Comments));
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.ComplaintHeaderId)
                            .MustAsync(async (id, ct) => await _queryRepository.ComplaintExistsAsync(id))
                            .WithMessage($"{nameof(SubmitQCReviewCommand.ComplaintHeaderId)} {rule.Error}")
                            .When(x => x.ComplaintHeaderId > 0);

                        RuleFor(x => x.PhysicalVerificationId)
                            .MustAsync(async (id, ct) => await _queryRepository.MiscMasterExistsAsync(id))
                            .WithMessage($"{nameof(SubmitQCReviewCommand.PhysicalVerificationId)} {rule.Error}")
                            .When(x => x.PhysicalVerificationId > 0);

                        RuleFor(x => x.ComplaintStatusId)
                            .MustAsync(async (id, ct) => await _queryRepository.MiscMasterExistsAsync(id!.Value))
                            .WithMessage($"{nameof(SubmitQCReviewCommand.ComplaintStatusId)} {rule.Error}")
                            .When(x => x.ComplaintStatusId.HasValue && x.ComplaintStatusId.Value > 0);

                        RuleFor(x => x.SeverityId)
                            .MustAsync(async (id, ct) => await _queryRepository.MiscMasterExistsAsync(id!.Value))
                            .WithMessage($"{nameof(SubmitQCReviewCommand.SeverityId)} {rule.Error}")
                            .When(x => x.SeverityId.HasValue && x.SeverityId.Value > 0);

                        RuleFor(x => x.CompensationStructureId)
                            .MustAsync(async (id, ct) => await _queryRepository.MiscMasterExistsAsync(id!.Value))
                            .WithMessage($"{nameof(SubmitQCReviewCommand.CompensationStructureId)} {rule.Error}")
                            .When(x => x.CompensationStructureId.HasValue && x.CompensationStructureId.Value > 0);

                        RuleFor(x => x.LabResponsiblePersonId)
                            .MustAsync(async (id, ct) => await _queryRepository.UserExistsAsync(id!.Value))
                            .WithMessage($"{nameof(SubmitQCReviewCommand.LabResponsiblePersonId)} {rule.Error}")
                            .When(x => x.LabResponsiblePersonId.HasValue && x.LabResponsiblePersonId.Value > 0);
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.ComplaintHeaderId)
                            .MustAsync(async (id, ct) => !await _queryRepository.ReviewAlreadyExistsAsync(id))
                            .WithMessage("A QC Review already exists for this complaint.")
                            .When(x => x.ComplaintHeaderId > 0);
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

            // Business rule: Complaint workflow must be approved before QC Review can be submitted
            RuleFor(x => x.ComplaintHeaderId)
                .MustAsync(async (id, ct) => await _queryRepository.IsComplaintApprovedAsync(id))
                .WithMessage("Cannot submit QC Review — complaint workflow is not yet approved.")
                .When(x => x.ComplaintHeaderId > 0);
        }
    }
}
