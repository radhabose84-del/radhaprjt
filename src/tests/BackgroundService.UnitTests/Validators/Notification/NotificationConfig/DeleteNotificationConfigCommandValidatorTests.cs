using BackgroundService.Application.Notification.Common.Interfaces.INotificationConfig;
using BackgroundService.Application.Notification.NotificationConfig.Command.DeleteNotificationConfig;
using BackgroundService.Presentation.Validation.NotificationConfig;
using FluentValidation.TestHelper;

namespace BackgroundService.UnitTests.Validators.Notification.NotificationConfig
{
    public sealed class DeleteNotificationConfigCommandValidatorTests
    {
        private readonly Mock<INotificationConfigQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private DeleteNotificationConfigCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        private void SetupAllAsyncMocks(int id = 1)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.SoftDeleteValidation(id)).ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = new DeleteNotificationConfigCommand { Id = 1 };
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_EmptyId_FailsValidation()
        {
            var command = new DeleteNotificationConfigCommand { Id = 0 };
            // Async rules still run for Id=0
            _mockQueryRepo.Setup(r => r.NotFoundAsync(0)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.SoftDeleteValidation(0)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_RecordNotFound_FailsValidation()
        {
            var command = new DeleteNotificationConfigCommand { Id = 99 };
            _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.SoftDeleteValidation(99)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }
    }
}
