using BackgroundService.Application.Notification.NotificationHierarchyAndEventRule.DTOs;
using BackgroundService.Presentation.Validation.NotificationHierarchyAndEventRule;
using FluentValidation.TestHelper;

namespace BackgroundService.UnitTests.Validators.Notification.NotificationHierarchyAndEventRule
{
    public sealed class NotificationEventRuleDtoValidatorTests
    {
        private NotificationEventRuleDtoValidator CreateValidator() => new();

        [Fact]
        public async Task Validate_ValidDto_PassesValidation()
        {
            var dto = new NotificationEventRuleDto
            {
                NotificationChannelId = 1,
                RecipientTypeId = 1,
                TemplateId = 1
            };

            var result = await CreateValidator().TestValidateAsync(dto);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_EmptyNotificationChannelId_FailsValidation()
        {
            var dto = new NotificationEventRuleDto { NotificationChannelId = 0, RecipientTypeId = 1, TemplateId = 1 };

            var result = await CreateValidator().TestValidateAsync(dto);
            result.ShouldHaveValidationErrorFor(x => x.NotificationChannelId);
        }

        [Fact]
        public async Task Validate_EmptyRecipientTypeId_FailsValidation()
        {
            var dto = new NotificationEventRuleDto { NotificationChannelId = 1, RecipientTypeId = 0, TemplateId = 1 };

            var result = await CreateValidator().TestValidateAsync(dto);
            result.ShouldHaveValidationErrorFor(x => x.RecipientTypeId);
        }

        [Fact]
        public async Task Validate_EmptyTemplateId_FailsValidation()
        {
            var dto = new NotificationEventRuleDto { NotificationChannelId = 1, RecipientTypeId = 1, TemplateId = 0 };

            var result = await CreateValidator().TestValidateAsync(dto);
            result.ShouldHaveValidationErrorFor(x => x.TemplateId);
        }
    }
}
