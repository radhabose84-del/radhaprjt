using FluentValidation;
using SalesManagement.Application.Common.Interfaces.IComplaintDepartmentFeedback;
using SalesManagement.Application.ComplaintDepartmentFeedback.Commands.UpdateFeedback;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.ComplaintDepartmentFeedback
{
    public class UpdateComplaintDepartmentFeedbackCommandValidator : AbstractValidator<UpdateComplaintDepartmentFeedbackCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IComplaintDepartmentFeedbackQueryRepository _queryRepository;

        public UpdateComplaintDepartmentFeedbackCommandValidator(
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
                        RuleFor(x => x.CorrectiveAction)
                            .NotNull()
                            .WithMessage($"{nameof(UpdateComplaintDepartmentFeedbackCommand.CorrectiveAction)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateComplaintDepartmentFeedbackCommand.CorrectiveAction)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.CorrectiveAction)
                            .MaximumLength(maxLengthCorrectiveAction)
                            .WithMessage($"{nameof(UpdateComplaintDepartmentFeedbackCommand.CorrectiveAction)} {rule.Error} {maxLengthCorrectiveAction} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.CorrectiveAction));

                        RuleFor(x => x.RootCauseText)
                            .MaximumLength(maxLengthRootCauseText)
                            .WithMessage($"{nameof(UpdateComplaintDepartmentFeedbackCommand.RootCauseText)} {rule.Error} {maxLengthRootCauseText} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.RootCauseText));

                        RuleFor(x => x.PreventiveAction)
                            .MaximumLength(maxLengthPreventiveAction)
                            .WithMessage($"{nameof(UpdateComplaintDepartmentFeedbackCommand.PreventiveAction)} {rule.Error} {maxLengthPreventiveAction} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.PreventiveAction));

                        RuleFor(x => x.Remarks)
                            .MaximumLength(maxLengthRemarks)
                            .WithMessage($"{nameof(UpdateComplaintDepartmentFeedbackCommand.Remarks)} {rule.Error} {maxLengthRemarks} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Remarks));
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage("Feedback not found.");
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.RootCauseCategoryId)
                            .MustAsync(async (id, ct) => await _queryRepository.MiscMasterExistsAsync(id!.Value))
                            .WithMessage("Root Cause Category not found.")
                            .When(x => x.RootCauseCategoryId.HasValue && x.RootCauseCategoryId.Value > 0);
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateComplaintDepartmentFeedbackCommand.IsActive)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }

            // Business rule: Root Cause mandatory
            RuleFor(x => x)
                .Must(x => !string.IsNullOrWhiteSpace(x.RootCauseText) || (x.RootCauseCategoryId.HasValue && x.RootCauseCategoryId.Value > 0))
                .WithMessage("Root Cause is mandatory — provide text or select a category.");

            // Business rule: QC Review workflow must be approved before feedback can be updated
            RuleFor(x => x.Id)
                .MustAsync(async (id, ct) => await _queryRepository.IsQCApprovedForFeedbackAsync(id))
                .WithMessage("Cannot update feedback — QC Review workflow is not yet approved.")
                .When(x => x.Id > 0);
        }
    }
}
