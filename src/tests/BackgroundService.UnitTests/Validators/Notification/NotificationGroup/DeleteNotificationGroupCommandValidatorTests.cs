using BackgroundService.Application.Notification.Common.Interfaces.INotificationGroup;
using BackgroundService.Application.Notification.NotificationGroup.Commands.DeleteNotificationGroup;
using BackgroundService.Presentation.Validation.NotificationGroup;
using FluentValidation.TestHelper;

namespace BackgroundService.UnitTests.Validators.Notification.NotificationGroup
{
    public sealed class DeleteNotificationGroupCommandValidatorTests
    {
        private readonly Mock<INotificationGroupQuery> _mockQueryRepo = new(MockBehavior.Strict);

        private DeleteNotificationGroupCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = new DeleteNotificationGroupCommand { Id = 1 };
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_EmptyId_FailsValidation()
        {
            var command = new DeleteNotificationGroupCommand { Id = 0 };
            // NotFound async rule still runs for Id=0
            _mockQueryRepo.Setup(r => r.NotFoundAsync(0)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NotFound_FailsValidation()
        {
            var command = new DeleteNotificationGroupCommand { Id = 99 };
            _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }
    }
}
