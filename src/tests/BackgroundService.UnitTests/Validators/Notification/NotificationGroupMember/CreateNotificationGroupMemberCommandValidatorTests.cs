using BackgroundService.Application.Notification.Common.Interfaces.INotificationGroupMembers;
using BackgroundService.Application.Notification.NotificationGroupMember.Commands.CreateNotificationGroupMember;
using BackgroundService.Presentation.Validation.NotificationGroupMember;
using FluentValidation.TestHelper;

namespace BackgroundService.UnitTests.Validators.Notification.NotificationGroupMember
{
    public sealed class CreateNotificationGroupMemberCommandValidatorTests
    {
        private readonly Mock<INotificationGroupMemberQuery> _mockQueryRepo = new(MockBehavior.Strict);

        private CreateNotificationGroupMemberCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        private void SetupAllAsyncMocks()
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(It.IsAny<int>(), It.IsAny<int>(), null))
                .ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = new CreateNotificationGroupMemberCommand { GroupId = 1, UserIds = new List<int> { 10, 20 } };
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_EmptyGroupId_FailsValidation()
        {
            var command = new CreateNotificationGroupMemberCommand { GroupId = 0, UserIds = new List<int> { 10 } };
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.GroupId);
        }

        [Fact]
        public async Task Validate_EmptyUserIds_FailsValidation()
        {
            var command = new CreateNotificationGroupMemberCommand { GroupId = 1, UserIds = new List<int>() };

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.UserIds);
        }

        [Fact]
        public async Task Validate_DuplicateUserIds_FailsValidation()
        {
            var command = new CreateNotificationGroupMemberCommand { GroupId = 1, UserIds = new List<int> { 10, 10 } };
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.UserIds)
                .WithErrorMessage("Duplicate UserIds are not allowed in the same request.");
        }

        [Fact]
        public async Task Validate_AlreadyExistingUser_FailsValidation()
        {
            var command = new CreateNotificationGroupMemberCommand { GroupId = 1, UserIds = new List<int> { 10 } };
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(1, 10, null)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveAnyValidationError();
        }
    }
}
