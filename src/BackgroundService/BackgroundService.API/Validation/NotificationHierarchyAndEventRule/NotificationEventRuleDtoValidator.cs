using FluentValidation;
using BackgroundService.Application.Notification.NotificationHierarchyAndEventRule.Commands.UpdateNotificationEventRule;
using BackgroundService.Application.Notification.NotificationHierarchyAndEventRule.DTOs;

namespace BackgroundService.API.Validation.NotificationHierarchyAndEventRule
{
    public class NotificationEventRuleDtoValidator : AbstractValidator<NotificationEventRuleDto>
    {
        public NotificationEventRuleDtoValidator()
        {
            RuleFor(x => x.NotificationChannelId)
                .NotEmpty().WithMessage("NotificationChannelId is required.");

            RuleFor(x => x.RecipientTypeId)
                .NotEmpty().WithMessage("RecipientTypeId is required.");

            RuleFor(x => x.TemplateId)
                .NotEmpty().WithMessage("TemplateId is required.");
        }
    }
}
