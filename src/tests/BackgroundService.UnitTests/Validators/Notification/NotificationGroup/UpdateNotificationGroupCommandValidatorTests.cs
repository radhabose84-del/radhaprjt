using BackgroundService.Application.Notification.Common.Interfaces.INotificationGroup;
using BackgroundService.Application.Notification.NotificationGroup.Commands.UpdateNotificationGroup;
using BackgroundService.Presentation.Validation.Common;
using BackgroundService.Presentation.Validation.NotificationGroup;
using FluentValidation.TestHelper;

namespace BackgroundService.UnitTests.Validators.Notification.NotificationGroup
{
    public sealed class UpdateNotificationGroupCommandValidatorTests
    {
        private readonly Mock<INotificationGroupQuery> _mockQueryRepo = new(MockBehavior.Strict);

        private UpdateNotificationGroupCommandValidator CreateValidator() =>
            new(new MaxLengthProvider(null!), _mockQueryRepo.Object);

        private void SetupAllAsyncMocks(int id = 1, string groupName = "UpdatedGroup")
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(groupName, id)).ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = new UpdateNotificationGroupCommand { Id = 1, GroupName = "UpdatedGroup", IsActive = 1 };
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyGroupName_FailsValidation(string? groupName)
        {
            var command = new UpdateNotificationGroupCommand { Id = 1, GroupName = groupName! };
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(true);
            // AlreadyExists async rule still runs - use It.IsAny for the anonymous type match
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.GroupName);
        }

        [Fact]
        public async Task Validate_NotFound_FailsValidation()
        {
            var command = new UpdateNotificationGroupCommand { Id = 99, GroupName = "UpdatedGroup" };
            _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync("UpdatedGroup", 99)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }
    }
}
