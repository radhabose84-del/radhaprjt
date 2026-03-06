using BackgroundService.Application.Notification.Common.Interfaces.INotificationLevelHierarchy;
using BackgroundService.Application.Notification.NotificationHierarchyAndEventRule.Commands.UpdateNotificationEventRule;
using FluentValidation;
using BackgroundService.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace BackgroundService.Presentation.Validation.NotificationHierarchyAndEventRule
{
    public class InsertNotificationHierarchyAndEventRuleCommandValidator 
        : AbstractValidator<InsertNotificationHierarchyAndEventRuleCommand>
    {
        public InsertNotificationHierarchyAndEventRuleCommandValidator(
            MaxLengthProvider maxLengthProvider,
            INotificationLevelHierarchyCommand repository)
        {
            var rules = ValidationRuleLoader.LoadValidationRules();
            var maxLength = maxLengthProvider.GetMaxLength<Domain.Entities.Notification.NotificationLevelHierarchy>("Description") ?? 250;

            if (rules == null || !rules.Any())
                throw new ArgumentException("Validation rules could not be loaded.");

            RuleFor(x => x.Dto).NotNull().WithMessage("Payload is required.");

            When(x => x.Dto != null, () =>
            {
                foreach (var rule in rules)
                {
                    switch (rule.Rule)
                    {
                        case "NotEmpty":
                            RuleFor(x => x.Dto.Description)
                                .NotEmpty()
                                .WithMessage("Description " + rule.Error);

                            RuleFor(x => x.Dto.NotificationConfigId)
                                .NotEmpty()
                                .WithMessage("NotificationConfigId " + rule.Error);

                            RuleFor(x => x.Dto.TargetTypeId)
                                .NotEmpty()
                                .WithMessage("TargetTypeId " + rule.Error);

                            RuleFor(x => x.Dto.TargetId)
                                .NotEmpty()
                                .WithMessage("TargetId " + rule.Error);

                            RuleFor(x => x.Dto.ApprovalModeId)
                                .NotEmpty()
                                .WithMessage("ApprovalModeId " + rule.Error);
                            break;

                        case "MaxLength":
                            RuleFor(x => x.Dto.Description)
                                .MaximumLength(maxLength)
                                .WithMessage("Description " + rule.Error);
                            break;

                        case "AlreadyExists":
                            RuleFor(x => x.Dto.NotificationConfigId)
                                .MustAsync(async (command, value, cancellation) =>
                                    !await repository.ExistsByCodeAsync(
                                        command.Dto.NotificationConfigId,
                                        command.Dto.TargetTypeId,
                                        command.Dto.TargetId))
                                .WithMessage("The combination already exists.");
                            break;
                    }
                }

                // 🔄 Validate each NotificationEventRule in the list
                RuleForEach(x => x.Dto.NotificationEventRules)
                    .SetValidator(new NotificationEventRuleDtoValidator());
            });
        }
    }
}
