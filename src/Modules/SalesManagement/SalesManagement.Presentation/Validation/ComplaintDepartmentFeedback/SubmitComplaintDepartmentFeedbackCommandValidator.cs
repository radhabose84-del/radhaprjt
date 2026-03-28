using FluentValidation;
using SalesManagement.Application.Common.Interfaces.IComplaintDepartmentFeedback;
using SalesManagement.Application.ComplaintDepartmentFeedback.Commands.SubmitFeedback;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.ComplaintDepartmentFeedback
{
    public class SubmitComplaintDepartmentFeedbackCommandValidator : AbstractValidator<SubmitComplaintDepartmentFeedbackCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IComplaintDepartmentFeedbackQueryRepository _queryRepository;

        public SubmitComplaintDepartmentFeedbackCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IComplaintDepartmentFeedbackQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            var maxLengthCorrectiveAction = maxLengthProvider.GetMaxLength<Domain.Entities.ComplaintDepartmentFeedback>("CorrectiveAction") ?? 2000;
            var maxLengthRootCauseText = maxLengthProvider.GetMaxLength<Domain.Entities.ComplaintDepartmentFeedback>("RootCauseText") ?? 2000;
            var maxLengthPreventiveAction = maxLengthProvider.GetMaxLength<Domain.Entities.ComplaintDepartmentFeedback>("PreventiveAction") ?? 2000;
            var maxLengthRemarks = maxLengthProvider.GetMaxLength<Domain.Entities.ComplaintDepartmentFeedback>("Remarks") ?? 1000;

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
                        RuleFor(x => x.AssignmentId)
                            .NotEmpty()
                            .WithMessage($"{nameof(SubmitComplaintDepartmentFeedbackCommand.AssignmentId)} {rule.Error}");

                        RuleFor(x => x.CorrectiveAction)
                            .NotNull()
                            .WithMessage($"{nameof(SubmitComplaintDepartmentFeedbackCommand.CorrectiveAction)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(SubmitComplaintDepartmentFeedbackCommand.CorrectiveAction)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.CorrectiveAction)
                            .MaximumLength(maxLengthCorrectiveAction)
                            .WithMessage($"{nameof(SubmitComplaintDepartmentFeedbackCommand.CorrectiveAction)} {rule.Error} {maxLengthCorrectiveAction} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.CorrectiveAction));

                        RuleFor(x => x.RootCauseText)
                            .MaximumLength(maxLengthRootCauseText)
                            .WithMessage($"{nameof(SubmitComplaintDepartmentFeedbackCommand.RootCauseText)} {rule.Error} {maxLengthRootCauseText} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.RootCauseText));

                        RuleFor(x => x.PreventiveAction)
                            .MaximumLength(maxLengthPreventiveAction)
                            .WithMessage($"{nameof(SubmitComplaintDepartmentFeedbackCommand.PreventiveAction)} {rule.Error} {maxLengthPreventiveAction} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.PreventiveAction));

                        RuleFor(x => x.Remarks)
                            .MaximumLength(maxLengthRemarks)
                            .WithMessage($"{nameof(SubmitComplaintDepartmentFeedbackCommand.Remarks)} {rule.Error} {maxLengthRemarks} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Remarks));
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.AssignmentId)
                            .MustAsync(async (id, ct) => await _queryRepository.AssignmentExistsAsync(id))
                            .WithMessage("Assignment not found.")
                            .When(x => x.AssignmentId > 0);

                        RuleFor(x => x.RootCauseCategoryId)
                            .MustAsync(async (id, ct) => await _queryRepository.MiscMasterExistsAsync(id!.Value))
                            .WithMessage("Root Cause Category not found.")
                            .When(x => x.RootCauseCategoryId.HasValue && x.RootCauseCategoryId.Value > 0);
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.AssignmentId)
                            .MustAsync(async (id, ct) => !await _queryRepository.FeedbackAlreadyExistsForAssignmentAsync(id))
                            .WithMessage("Feedback already submitted for this assignment.")
                            .When(x => x.AssignmentId > 0);
                        break;

                    default:
                        break;
                }
            }

            // Business rule: Root Cause mandatory — either text or category must be provided
            RuleFor(x => x)
                .Must(x => !string.IsNullOrWhiteSpace(x.RootCauseText) || (x.RootCauseCategoryId.HasValue && x.RootCauseCategoryId.Value > 0))
                .WithMessage("Root Cause is mandatory — provide text or select a category.");
        }
    }
}
