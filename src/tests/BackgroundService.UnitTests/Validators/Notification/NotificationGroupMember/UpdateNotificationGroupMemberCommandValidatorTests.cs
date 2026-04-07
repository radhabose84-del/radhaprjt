using BackgroundService.Application.Notification.Common.Interfaces.INotificationGroupMembers;
using BackgroundService.Application.Notification.NotificationGroupMember.Commands.UpdateNotificationGroupMember;
using BackgroundService.Presentation.Validation.NotificationGroupMember;
using FluentValidation.TestHelper;

namespace BackgroundService.UnitTests.Validators.Notification.NotificationGroupMember
{
    public sealed class UpdateNotificationGroupMemberCommandValidatorTests
    {
        private readonly Mock<INotificationGroupMemberQuery> _mockQueryRepo = new(MockBehavior.Strict);

        private UpdateNotificationGroupMemberCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        private void SetupAllAsyncMocks(int groupId = 1)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(groupId)).ReturnsAsync(true);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = new UpdateNotificationGroupMemberCommand { GroupId = 1, UserIds = new List<int> { 10, 20 }, IsActive = 1 };
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroGroupId_FailsValidation()
        {
            var command = new UpdateNotificationGroupMemberCommand { GroupId = 0, UserIds = new List<int> { 10 }, IsActive = 1 };
            // NotFound async rule still runs for GroupId=0
            _mockQueryRepo.Setup(r => r.NotFoundAsync(0)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.GroupId);
        }

        [Fact]
        public async Task Validate_EmptyUserIds_FailsValidation()
        {
            var command = new UpdateNotificationGroupMemberCommand { GroupId = 1, UserIds = new List<int>(), IsActive = 1 };
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.UserIds);
        }

        [Fact]
        public async Task Validate_NotFound_FailsValidation()
        {
            var command = new UpdateNotificationGroupMemberCommand { GroupId = 99, UserIds = new List<int> { 10 }, IsActive = 1 };
            _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.GroupId);
        }
    }
}
