using FluentValidation;
using SalesManagement.Application.Common.Interfaces.IComplaintDepartmentFeedback;
using SalesManagement.Application.ComplaintDepartmentFeedback.Commands.RequestRework;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.ComplaintDepartmentFeedback
{
    public class RequestReworkCommandValidator : AbstractValidator<RequestReworkCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IComplaintDepartmentFeedbackQueryRepository _queryRepository;

        public RequestReworkCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IComplaintDepartmentFeedbackQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            var maxLengthReworkReason = maxLengthProvider.GetMaxLength<Domain.Entities.ComplaintDepartmentFeedback>("ReworkReason") ?? 1000;

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
                        RuleFor(x => x.FeedbackId)
                            .NotEmpty()
                            .WithMessage($"{nameof(RequestReworkCommand.FeedbackId)} {rule.Error}");

                        RuleFor(x => x.ReworkReason)
                            .NotNull()
                            .WithMessage($"{nameof(RequestReworkCommand.ReworkReason)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(RequestReworkCommand.ReworkReason)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.ReworkReason)
                            .MaximumLength(maxLengthReworkReason)
                            .WithMessage($"{nameof(RequestReworkCommand.ReworkReason)} {rule.Error} {maxLengthReworkReason} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.ReworkReason));
                        break;

                    case "NotFound":
                        RuleFor(x => x.FeedbackId)
                            .GreaterThan(0).WithMessage("Valid FeedbackId is required.")
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage("Feedback not found.");
                        break;

                    default:
                        break;
                }
            }

            // Business rule: QC Review workflow must be approved before rework can be requested
            RuleFor(x => x.FeedbackId)
                .MustAsync(async (id, ct) => await _queryRepository.IsQCApprovedForFeedbackAsync(id))
                .WithMessage("Cannot request rework — QC Review workflow is not yet approved.")
                .When(x => x.FeedbackId > 0);
        }
    }
}
