using BackgroundService.Application.Notification.Common.Interfaces.INotificationConfig;
using BackgroundService.Application.Notification.NotificationConfig.Command.UpdateNotificationConfig;
using BackgroundService.Presentation.Validation.Common;
using BackgroundService.Presentation.Validation.NotificationConfig;
using FluentValidation.TestHelper;

namespace BackgroundService.UnitTests.Validators.Notification.NotificationConfig
{
    public sealed class UpdateNotificationConfigCommandValidatorTests
    {
        private readonly Mock<INotificationConfigCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<INotificationConfigQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private UpdateNotificationConfigCommandValidator CreateValidator() =>
            new(new MaxLengthProvider(null!), _mockCommandRepo.Object, _mockQueryRepo.Object);

        private void SetupAllAsyncMocks(int id = 1, string? moduleName = "TestModule", int eventTypeId = 1)
        {
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(id))
                .ReturnsAsync(true);

            _mockCommandRepo
                .Setup(r => r.IsNameDuplicateAsync(moduleName, eventTypeId, id))
                .ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = new UpdateNotificationConfigCommand { Id = 1, ModuleName = "TestModule", NotificationEventTypeId = 1, IsActive = 1 };
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyModuleName_FailsValidation(string? moduleName)
        {
            var command = new UpdateNotificationConfigCommand { Id = 1, ModuleName = moduleName, NotificationEventTypeId = 1 };
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(true);
            // AlreadyExists case has .NotEmpty() then .MustAsync - need async mock
            _mockCommandRepo
                .Setup(r => r.IsNameDuplicateAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.ModuleName);
        }

        [Fact]
        public async Task Validate_RecordNotFound_FailsValidation()
        {
            var command = new UpdateNotificationConfigCommand { Id = 99, ModuleName = "TestModule", NotificationEventTypeId = 1 };
            _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(false);
            _mockCommandRepo.Setup(r => r.IsNameDuplicateAsync("TestModule", 1, 99)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }
    }
}
