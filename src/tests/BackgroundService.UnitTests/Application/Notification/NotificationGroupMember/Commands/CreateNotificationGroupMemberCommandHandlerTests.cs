using AutoMapper;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationGroupMembers;
using BackgroundService.Application.Notification.Exceptions;
using BackgroundService.Application.Notification.NotificationGroupMember.Commands.CreateNotificationGroupMember;
using BackgroundService.Domain.Entities.Notification;

namespace BackgroundService.UnitTests.Application.Notification.NotificationGroupMember.Commands
{
    public sealed class CreateNotificationGroupMemberCommandHandlerTests
    {
        private readonly Mock<INotificationGroupMemberCommand> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private CreateNotificationGroupMemberCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object);

        private static CreateNotificationGroupMemberCommand ValidCommand() =>
            new() { GroupId = 1, UserIds = new List<int> { 10, 20, 30 } };

        [Fact]
        public async Task Handle_ValidCommand_ReturnsCount()
        {
            _mockCommandRepo
                .Setup(r => r.CreateMultipleAsync(It.IsAny<IEnumerable<NotificationGroupMembers>>()))
                .ReturnsAsync(3);

            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            result.Should().Be(3);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateMultipleAsyncOnce()
        {
            _mockCommandRepo
                .Setup(r => r.CreateMultipleAsync(It.IsAny<IEnumerable<NotificationGroupMembers>>()))
                .ReturnsAsync(3);

            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateMultipleAsync(It.IsAny<IEnumerable<NotificationGroupMembers>>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_EmptyUserIds_ThrowsExceptionRules()
        {
            var command = new CreateNotificationGroupMemberCommand { GroupId = 1, UserIds = new List<int>() };

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);
            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*At least one UserId*");
        }

        [Fact]
        public async Task Handle_NullUserIds_ThrowsExceptionRules()
        {
            var command = new CreateNotificationGroupMemberCommand { GroupId = 1, UserIds = null! };

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);
            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*At least one UserId*");
        }

        [Fact]
        public async Task Handle_CreateReturnsZero_ThrowsExceptionRules()
        {
            _mockCommandRepo
                .Setup(r => r.CreateMultipleAsync(It.IsAny<IEnumerable<NotificationGroupMembers>>()))
                .ReturnsAsync(0);

            Func<Task> act = async () => await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*Creation Failed*");
        }
    }
}
