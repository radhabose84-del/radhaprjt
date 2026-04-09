using BackgroundService.Application.Notification.Common.Interfaces.INotificationLevelHierarchy;
using BackgroundService.Application.Notification.NotificationHierarchyAndEventRule.Commands.UpdateNotificationEventRule;
using BackgroundService.Application.Notification.NotificationHierarchyAndEventRule.DTOs;
using BackgroundService.Presentation.Validation.Common;
using BackgroundService.Presentation.Validation.NotificationHierarchyAndEventRule;
using FluentValidation.TestHelper;

namespace BackgroundService.UnitTests.Validators.Notification.NotificationHierarchyAndEventRule
{
    public sealed class InsertNotificationHierarchyAndEventRuleCommandValidatorTests
    {
        private readonly Mock<INotificationLevelHierarchyCommand> _mockRepo = new(MockBehavior.Strict);

        private InsertNotificationHierarchyAndEventRuleCommandValidator CreateValidator() =>
            new(new MaxLengthProvider(null!), _mockRepo.Object);

        private static InsertNotificationHierarchyAndEventRuleCommand ValidCommand() =>
            new(new NotificationHierarchyAndEventRuleDto
            {
                NotificationConfigId = 1,
                TargetTypeId = 2,
                TargetId = 3,
                ApprovalModeId = 1,
                Description = "Test Description",
                NotificationEventRules = new List<NotificationEventRuleDto>
                {
                    new() { NotificationChannelId = 1, RecipientTypeId = 1, TemplateId = 1 }
                }
            });

        private void SetupAllAsyncMocks(int configId = 1, int targetTypeId = 2, int targetId = 3)
        {
            _mockRepo.Setup(r => r.ExistsByCodeAsync(configId, targetTypeId, targetId)).ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            SetupAllAsyncMocks();
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_NullDto_FailsValidation()
        {
            var command = new InsertNotificationHierarchyAndEventRuleCommand(null!);

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.Dto);
        }

        [Fact]
        public async Task Validate_EmptyDescription_FailsValidation()
        {
            var dto = new NotificationHierarchyAndEventRuleDto
            {
                NotificationConfigId = 1, TargetTypeId = 2, TargetId = 3, ApprovalModeId = 1,
                Description = "",
                NotificationEventRules = new List<NotificationEventRuleDto>
                {
                    new() { NotificationChannelId = 1, RecipientTypeId = 1, TemplateId = 1 }
                }
            };
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(new InsertNotificationHierarchyAndEventRuleCommand(dto));
            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task Validate_DuplicateCombination_FailsValidation()
        {
            _mockRepo.Setup(r => r.ExistsByCodeAsync(1, 2, 3)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldHaveAnyValidationError();
        }
    }
}
