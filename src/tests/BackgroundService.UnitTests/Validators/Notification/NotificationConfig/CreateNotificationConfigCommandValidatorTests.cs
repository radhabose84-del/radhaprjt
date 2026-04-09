using BackgroundService.Application.Notification.Common.Interfaces.INotificationConfig;
using BackgroundService.Application.Notification.NotificationConfig.Command.CreateNotificationConfig;
using BackgroundService.Presentation.Validation.Common;
using BackgroundService.Presentation.Validation.NotificationConfig;
using FluentValidation.TestHelper;

namespace BackgroundService.UnitTests.Validators.Notification.NotificationConfig
{
    public sealed class CreateNotificationConfigCommandValidatorTests
    {
        private readonly Mock<INotificationConfigCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);

        private CreateNotificationConfigCommandValidator CreateValidator() =>
            new(new MaxLengthProvider(null!), _mockCommandRepo.Object);

        private void SetupAllAsyncMocks(string? moduleName = "TestModule", int eventTypeId = 1)
        {
            _mockCommandRepo
                .Setup(r => r.ExistsByCodeAsync(moduleName, eventTypeId))
                .ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = new CreateNotificationConfigCommand { ModuleName = "TestModule", NotificationEventTypeId = 1 };
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyModuleName_FailsValidation(string? moduleName)
        {
            var command = new CreateNotificationConfigCommand { ModuleName = moduleName, NotificationEventTypeId = 1 };
            // AlreadyExists case also has .NotEmpty() then .MustAsync - setup the async mock
            _mockCommandRepo
                .Setup(r => r.ExistsByCodeAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.ModuleName);
        }

        [Fact]
        public async Task Validate_EmptyNotificationEventTypeId_FailsValidation()
        {
            var command = new CreateNotificationConfigCommand { ModuleName = "TestModule", NotificationEventTypeId = 0 };
            // AlreadyExists MustAsync still runs
            _mockCommandRepo
                .Setup(r => r.ExistsByCodeAsync("TestModule", 0))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.NotificationEventTypeId);
        }

        [Fact]
        public async Task Validate_DuplicateModuleName_FailsValidation()
        {
            var command = new CreateNotificationConfigCommand { ModuleName = "ExistingModule", NotificationEventTypeId = 1 };

            _mockCommandRepo
                .Setup(r => r.ExistsByCodeAsync("ExistingModule", 1))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.ModuleName)
                .WithErrorMessage("A ModuleName already exists in this Event Type.");
        }
    }
}
