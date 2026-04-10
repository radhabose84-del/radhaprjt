using AutoMapper;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationWhatsAppGroup;
using BackgroundService.Application.Notification.Exceptions;
using BackgroundService.Application.Notification.NotificationWhatsAppGroup.Commands.DeleteNotificationWhatsAppGroup;
using NotificationWhatsAppGroupEntity = BackgroundService.Core.Domain.Entities.Notifications.NotificationWhatsAppGroup;

namespace BackgroundService.UnitTests.Application.Notification.NotificationWhatsAppGroup.Commands
{
    public sealed class DeleteNotificationWhatsAppGroupCommandHandlerTests
    {
        private readonly Mock<INotificationWhatsAppGroupCommand> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private DeleteNotificationWhatsAppGroupCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_ValidCommand_ReturnsTrue()
        {
            _mockCommandRepo
                .Setup(r => r.DeleteAsync(1, It.IsAny<NotificationWhatsAppGroupEntity>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Handle(
                new DeleteNotificationWhatsAppGroupCommand { Id = 1 },
                CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_DeleteFails_ThrowsExceptionRules()
        {
            _mockCommandRepo
                .Setup(r => r.DeleteAsync(99, It.IsAny<NotificationWhatsAppGroupEntity>()))
                .ReturnsAsync(false);

            Func<Task> act = async () => await CreateSut().Handle(
                new DeleteNotificationWhatsAppGroupCommand { Id = 99 },
                CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*Delete failed*");
        }
    }
}
