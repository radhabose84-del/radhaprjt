using AutoMapper;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationWhatsAppGroup;
using BackgroundService.Application.Notification.Exceptions;
using BackgroundService.Application.Notification.NotificationWhatsAppGroup.Commands.CreateNotificationWhatsAppGroup;
using NotificationWhatsAppGroupEntity = BackgroundService.Core.Domain.Entities.Notifications.NotificationWhatsAppGroup;

namespace BackgroundService.UnitTests.Application.Notification.NotificationWhatsAppGroup.Commands
{
    public sealed class CreateNotificationWhatsAppGroupCommandHandlerTests
    {
        private readonly Mock<INotificationWhatsAppGroupCommand> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private CreateNotificationWhatsAppGroupCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object);

        private static CreateNotificationWhatsAppGroupCommand ValidCommand() =>
            new() { DepartmentId = 1, GroupName = "TestGroup", ApiKey = "key123" };

        private void SetupHappyPath(int newId = 1)
        {
            _mockCommandRepo
                .Setup(r => r.ExistsByNameAsync("TestGroup", 1, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockMapper
                .Setup(m => m.Map<NotificationWhatsAppGroupEntity>(It.IsAny<CreateNotificationWhatsAppGroupCommand>()))
                .Returns(new NotificationWhatsAppGroupEntity { GroupName = "TestGroup" });

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<NotificationWhatsAppGroupEntity>()))
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
        public async Task Handle_DuplicateName_ThrowsExceptionRules()
        {
            _mockCommandRepo
                .Setup(r => r.ExistsByNameAsync("TestGroup", 1, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            Func<Task> act = async () => await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*already exists*");
        }

        [Fact]
        public async Task Handle_CreateReturnsZero_ThrowsExceptionRules()
        {
            SetupHappyPath(0);

            Func<Task> act = async () => await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*creation failed*");
        }
    }
}
