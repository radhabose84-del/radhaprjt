using BackgroundService.Application.Notification.Common.Interfaces.INotificationGroup;
using BackgroundService.Application.Notification.NotificationGroup.Commands.CreateNotificationGroup;
using BackgroundService.Presentation.Validation.Common;
using BackgroundService.Presentation.Validation.NotificationGroup;
using FluentValidation.TestHelper;

namespace BackgroundService.UnitTests.Validators.Notification.NotificationGroup
{
    public sealed class CreateNotificationGroupCommandValidatorTests
    {
        private readonly Mock<INotificationGroupQuery> _mockQueryRepo = new(MockBehavior.Strict);

        private CreateNotificationGroupCommandValidator CreateValidator() =>
            new(new MaxLengthProvider(null!), _mockQueryRepo.Object);

        private void SetupAllAsyncMocks(string groupName = "TestGroup")
        {
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(groupName, null)).ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = new CreateNotificationGroupCommand { GroupName = "TestGroup" };
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyGroupName_FailsValidation(string? groupName)
        {
            var command = new CreateNotificationGroupCommand { GroupName = groupName! };
            // AlreadyExists async rule still runs
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), null))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.GroupName);
        }

        [Fact]
        public async Task Validate_DuplicateGroupName_FailsValidation()
        {
            var command = new CreateNotificationGroupCommand { GroupName = "ExistingGroup" };
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync("ExistingGroup", null)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.GroupName);
        }
    }
}
