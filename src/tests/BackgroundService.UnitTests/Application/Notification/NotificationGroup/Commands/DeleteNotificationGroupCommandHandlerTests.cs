using AutoMapper;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationGroup;
using BackgroundService.Application.Notification.Exceptions;
using BackgroundService.Application.Notification.NotificationGroup.Commands.DeleteNotificationGroup;
using MediatR;

namespace BackgroundService.UnitTests.Application.Notification.NotificationGroup.Commands
{
    public sealed class DeleteNotificationGroupCommandHandlerTests
    {
        private readonly Mock<INotificationGroupCommand> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private DeleteNotificationGroupCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(bool result = true)
        {
            var entity = new BackgroundService.Domain.Entities.Notification.NotificationGroup { Id = 1 };

            _mockMapper
                .Setup(m => m.Map<BackgroundService.Domain.Entities.Notification.NotificationGroup>(It.IsAny<DeleteNotificationGroupCommand>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<BackgroundService.Domain.Entities.Notification.NotificationGroup>()))
                .ReturnsAsync(result);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsTrue()
        {
            SetupHappyPath(true);
            var result = await CreateSut().Handle(new DeleteNotificationGroupCommand { Id = 1 }, CancellationToken.None);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsDeleteAsyncOnce()
        {
            SetupHappyPath(true);
            await CreateSut().Handle(new DeleteNotificationGroupCommand { Id = 1 }, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<BackgroundService.Domain.Entities.Notification.NotificationGroup>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_DeleteReturnsFalse_ThrowsExceptionRules()
        {
            SetupHappyPath(false);

            Func<Task> act = async () => await CreateSut().Handle(new DeleteNotificationGroupCommand { Id = 99 }, CancellationToken.None);
            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*deletion failed*");
        }
    }
}
