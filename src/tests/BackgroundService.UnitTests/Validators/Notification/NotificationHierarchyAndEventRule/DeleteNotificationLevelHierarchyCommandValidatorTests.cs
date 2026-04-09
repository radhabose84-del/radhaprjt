using BackgroundService.Application.Notification.Common.Interfaces.INotificationLevelHierarchy;
using BackgroundService.Application.Notification.NotificationHierarchyAndEventRule.Queries.DeleteNotificationEventRule;
using BackgroundService.Presentation.Validation.NotificationHierarchyAndEventRule;
using FluentValidation.TestHelper;

namespace BackgroundService.UnitTests.Validators.Notification.NotificationHierarchyAndEventRule
{
    public sealed class DeleteNotificationLevelHierarchyCommandValidatorTests
    {
        private readonly Mock<INotificationLevelHierarchyCommand> _mockRepo = new(MockBehavior.Strict);

        private DeleteNotificationLevelHierarchyCommandValidator CreateValidator() =>
            new(_mockRepo.Object);

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = new DeleteNotificationLevelHierarchyCommand { Id = 1 };
            // Validator: !await NotFoundAsync(id) => !false = true => pass
            _mockRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_EmptyId_FailsValidation()
        {
            var command = new DeleteNotificationLevelHierarchyCommand { Id = 0 };
            // NotFound async rule still runs for Id=0
            _mockRepo.Setup(r => r.NotFoundAsync(0)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NotFound_FailsValidation()
        {
            var command = new DeleteNotificationLevelHierarchyCommand { Id = 99 };
            // Validator: !await NotFoundAsync(id) => !true = false => fail
            _mockRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }
    }
}
