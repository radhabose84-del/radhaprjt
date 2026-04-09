using AutoMapper;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationWhatsAppGroup;
using BackgroundService.Application.Notification.Exceptions;
using BackgroundService.Application.Notification.NotificationWhatsAppGroup.Commands.UpdateNotificationWhatsAppGroup;
using NotificationWhatsAppGroupEntity = BackgroundService.Core.Domain.Entities.Notifications.NotificationWhatsAppGroup;

namespace BackgroundService.UnitTests.Application.Notification.NotificationWhatsAppGroup.Commands
{
    public sealed class UpdateNotificationWhatsAppGroupCommandHandlerTests
    {
        private readonly Mock<INotificationWhatsAppGroupCommand> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private UpdateNotificationWhatsAppGroupCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object);

        private static UpdateNotificationWhatsAppGroupCommand ValidCommand() =>
            new() { Id = 1, DepartmentId = 1, GroupName = "UpdatedGroup", ApiKey = "key123", IsActive = 1 };

        private void SetupHappyPath()
        {
            _mockCommandRepo
                .Setup(r => r.ExistsByNameAsync("UpdatedGroup", 1, 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockMapper
                .Setup(m => m.Map<NotificationWhatsAppGroupEntity>(It.IsAny<UpdateNotificationWhatsAppGroupCommand>()))
                .Returns(new NotificationWhatsAppGroupEntity { Id = 1, GroupName = "UpdatedGroup" });

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<NotificationWhatsAppGroupEntity>()))
                .ReturnsAsync(true);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsTrue()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_DuplicateName_ThrowsExceptionRules()
        {
            _mockCommandRepo
                .Setup(r => r.ExistsByNameAsync("UpdatedGroup", 1, 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            Func<Task> act = async () => await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*already exists*");
        }

        [Fact]
        public async Task Handle_UpdateFails_ThrowsExceptionRules()
        {
            _mockCommandRepo
                .Setup(r => r.ExistsByNameAsync("UpdatedGroup", 1, 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockMapper
                .Setup(m => m.Map<NotificationWhatsAppGroupEntity>(It.IsAny<UpdateNotificationWhatsAppGroupCommand>()))
                .Returns(new NotificationWhatsAppGroupEntity());

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<NotificationWhatsAppGroupEntity>()))
                .ReturnsAsync(false);

            Func<Task> act = async () => await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*update failed*");
        }
    }
}
