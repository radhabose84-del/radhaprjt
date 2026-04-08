using BackgroundService.Application.Notification.Common.Interfaces.INotificationLevelHierarchy;
using BackgroundService.Application.Notification.NotificationHierarchyAndEventRule.Commands.UpdateNotificationEventRule;
using BackgroundService.Application.Notification.NotificationHierarchyAndEventRule.DTOs;
using BackgroundService.Presentation.Validation.Common;
using BackgroundService.Presentation.Validation.NotificationHierarchyAndEventRule;
using FluentValidation.TestHelper;

namespace BackgroundService.UnitTests.Validators.Notification.NotificationHierarchyAndEventRule
{
    public sealed class UpdateNotificationHierarchyAndEventRuleCommandValidatorTests
    {
        private readonly Mock<INotificationLevelHierarchyCommand> _mockRepo = new(MockBehavior.Strict);

        private UpdateNotificationHierarchyAndEventRuleCommandValidator CreateValidator() =>
            new(new MaxLengthProvider(null!), _mockRepo.Object);

        private static UpdateNotificationHierarchyAndEventRuleCommand ValidCommand() =>
            new()
            {
                NotificationLevelHierarchyId = 1,
                NotificationConfigId = 1,
                TargetTypeId = 2,
                TargetId = 3,
                ApprovalModeId = 1,
                Description = "Updated",
                IsActive = 1,
                NotificationEventRules = new List<NotificationEventRuleDto>
                {
                    new() { NotificationChannelId = 1, RecipientTypeId = 1, TemplateId = 1 }
                }
            };

        private void SetupAllAsyncMocks(int configId = 1, int targetTypeId = 2, int targetId = 3, int hierarchyId = 1)
        {
            _mockRepo.Setup(r => r.ExistsByCodeExcludingIdAsync(configId, targetTypeId, targetId, hierarchyId)).ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            SetupAllAsyncMocks();
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_EmptyDescription_FailsValidation()
        {
            var command = ValidCommand();
            command.Description = "";
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public async Task Validate_EmptyNotificationLevelHierarchyId_FailsValidation()
        {
            var command = ValidCommand();
            command.NotificationLevelHierarchyId = 0;
            _mockRepo.Setup(r => r.ExistsByCodeExcludingIdAsync(1, 2, 3, 0)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.NotificationLevelHierarchyId);
        }

        [Fact]
        public async Task Validate_DuplicateCombination_FailsValidation()
        {
            _mockRepo.Setup(r => r.ExistsByCodeExcludingIdAsync(1, 2, 3, 1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldHaveAnyValidationError();
        }
    }
}
