using BackgroundService.Application.Notification.Common.Interfaces.INotificationLevelHierarchy;
using BackgroundService.Application.Notification.NotificationHierarchyAndEventRule.Commands.UpdateNotificationEventRule;
using FluentValidation;
using Shared.Validation.Common;
using BackgroundService.Presentation.Validation.Common;

namespace BackgroundService.Presentation.Validation.NotificationHierarchyAndEventRule
{
    public class UpdateNotificationHierarchyAndEventRuleCommandValidator : AbstractValidator<UpdateNotificationHierarchyAndEventRuleCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly INotificationLevelHierarchyCommand _repository;

        public UpdateNotificationHierarchyAndEventRuleCommandValidator(
            MaxLengthProvider maxLengthProvider,
            INotificationLevelHierarchyCommand repository)
        {
            _repository = repository;
            _validationRules = ValidationRuleLoader.LoadValidationRules();

            var maxLength = maxLengthProvider.GetMaxLength<BackgroundService.Domain.Entities.Notification.NotificationLevelHierarchy>("Description") ?? 250;

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.Description)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateNotificationHierarchyAndEventRuleCommand.Description)} {rule.Error}");

                        RuleFor(x => x.NotificationConfigId)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateNotificationHierarchyAndEventRuleCommand.NotificationConfigId)} {rule.Error}");

                        RuleFor(x => x.TargetTypeId)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateNotificationHierarchyAndEventRuleCommand.TargetTypeId)} {rule.Error}");

                        RuleFor(x => x.TargetId)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateNotificationHierarchyAndEventRuleCommand.TargetId)} {rule.Error}");

                        RuleFor(x => x.ApprovalModeId)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateNotificationHierarchyAndEventRuleCommand.ApprovalModeId)} {rule.Error}");

                        RuleFor(x => x.NotificationLevelHierarchyId)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateNotificationHierarchyAndEventRuleCommand.NotificationLevelHierarchyId)} {rule.Error}");

                        break;

                    case "MaxLength":
                        RuleFor(x => x.Description)
                            .MaximumLength(maxLength)
                            .WithMessage($"{nameof(UpdateNotificationHierarchyAndEventRuleCommand.Description)} {rule.Error}");
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.NotificationConfigId)
                            .MustAsync(async (command, configId, cancellation) =>
                                !await _repository.ExistsByCodeExcludingIdAsync(
                                    configId,
                                    command.TargetTypeId,
                                    command.TargetId,
                                    command.NotificationLevelHierarchyId))
                            .WithMessage("This combination already exists.");
                        break;
                }
            }
        }
    }
}
