using AutoMapper;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationGroupMembers;
using BackgroundService.Application.Notification.Exceptions;
using BackgroundService.Application.Notification.NotificationGroupMember.Commands.UpdateNotificationGroupMember;

namespace BackgroundService.UnitTests.Application.Notification.NotificationGroupMember.Commands
{
    public sealed class UpdateNotificationGroupMemberCommandHandlerTests
    {
        private readonly Mock<INotificationGroupMemberCommand> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private UpdateNotificationGroupMemberCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object);

        private static UpdateNotificationGroupMemberCommand ValidCommand() =>
            new() { GroupId = 1, UserIds = new List<int> { 10, 20 }, IsActive = 1 };

        [Fact]
        public async Task Handle_ValidCommand_ReturnsTrue()
        {
            _mockCommandRepo
                .Setup(r => r.UpdateMultipleAsync(1, It.IsAny<List<int>>(), 1))
                .ReturnsAsync(true);

            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_UpdateReturnsFalse_ThrowsExceptionRules()
        {
            _mockCommandRepo
                .Setup(r => r.UpdateMultipleAsync(1, It.IsAny<List<int>>(), 1))
                .ReturnsAsync(false);

            Func<Task> act = async () => await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*update failed*");
        }
    }
}
