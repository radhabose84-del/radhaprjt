using AutoMapper;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationGroup;
using BackgroundService.Application.Notification.Exceptions;
using BackgroundService.Application.Notification.NotificationGroup.Commands.CreateNotificationGroup;
using MediatR;

namespace BackgroundService.UnitTests.Application.Notification.NotificationGroup.Commands
{
    public sealed class CreateNotificationGroupCommandHandlerTests
    {
        private readonly Mock<INotificationGroupCommand> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private CreateNotificationGroupCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private static CreateNotificationGroupCommand ValidCommand() =>
            new() { GroupName = "TestGroup" };

        private void SetupHappyPath(int newId = 1)
        {
            var entity = new BackgroundService.Domain.Entities.Notification.NotificationGroup { Id = newId, GroupName = "TestGroup" };

            _mockMapper
                .Setup(m => m.Map<BackgroundService.Domain.Entities.Notification.NotificationGroup>(It.IsAny<CreateNotificationGroupCommand>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<BackgroundService.Domain.Entities.Notification.NotificationGroup>()))
                .ReturnsAsync(newId);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            SetupHappyPath(5);
            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            result.Should().Be(5);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateAsyncOnce()
        {
            SetupHappyPath(1);
            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<BackgroundService.Domain.Entities.Notification.NotificationGroup>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_CreateReturnsZero_ThrowsExceptionRules()
        {
            var entity = new BackgroundService.Domain.Entities.Notification.NotificationGroup { Id = 0 };

            _mockMapper
                .Setup(m => m.Map<BackgroundService.Domain.Entities.Notification.NotificationGroup>(It.IsAny<CreateNotificationGroupCommand>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<BackgroundService.Domain.Entities.Notification.NotificationGroup>()))
                .ReturnsAsync(0);

            Func<Task> act = async () => await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*Creation Failed*");
        }
    }
}
