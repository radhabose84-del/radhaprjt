using AutoMapper;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationGroup;
using BackgroundService.Application.Notification.Exceptions;
using BackgroundService.Application.Notification.NotificationGroup.Commands.UpdateNotificationGroup;
using MediatR;

namespace BackgroundService.UnitTests.Application.Notification.NotificationGroup.Commands
{
    public sealed class UpdateNotificationGroupCommandHandlerTests
    {
        private readonly Mock<INotificationGroupCommand> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private UpdateNotificationGroupCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private static UpdateNotificationGroupCommand ValidCommand() =>
            new() { Id = 1, GroupName = "UpdatedGroup", IsActive = 1 };

        private void SetupHappyPath(bool result = true)
        {
            var entity = new BackgroundService.Domain.Entities.Notification.NotificationGroup { Id = 1, GroupName = "UpdatedGroup" };

            _mockMapper
                .Setup(m => m.Map<BackgroundService.Domain.Entities.Notification.NotificationGroup>(It.IsAny<UpdateNotificationGroupCommand>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<BackgroundService.Domain.Entities.Notification.NotificationGroup>()))
                .ReturnsAsync(result);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsTrue()
        {
            SetupHappyPath(true);
            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateAsyncOnce()
        {
            SetupHappyPath(true);
            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.UpdateAsync(It.IsAny<BackgroundService.Domain.Entities.Notification.NotificationGroup>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_UpdateReturnsFalse_ThrowsExceptionRules()
        {
            SetupHappyPath(false);

            Func<Task> act = async () => await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*update failed*");
        }
    }
}
