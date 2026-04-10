using AutoMapper;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationConfig;
using BackgroundService.Application.Notification.NotificationConfig.Queries.GetAllNotificationConfig;
using BackgroundService.Application.Notification.NotificationConfig.Queries.GetNotificationConfigById;
using BackgroundService.Domain.Events;
using MediatR;

namespace BackgroundService.UnitTests.Application.Notification.NotificationConfig.Queries
{
    public sealed class GetNotificationConfigByIdQueryHandlerTests
    {
        private readonly Mock<INotificationConfigQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private GetNotificationConfigByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_ValidId_ReturnsDto()
        {
            var dto = new NotificationConfigDto { Id = 1, ModuleName = "Test" };

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(dto);

            _mockMapper
                .Setup(m => m.Map<NotificationConfigDto>(It.IsAny<NotificationConfigDto>()))
                .Returns(dto);

            var result = await CreateSut().Handle(
                new GetNotificationConfigByIdQuery { Id = 1 },
                CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.ModuleName.Should().Be("Test");
        }

        [Fact]
        public async Task Handle_NotFound_ThrowsKeyNotFoundException()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((NotificationConfigDto?)null);

            Func<Task> act = async () => await CreateSut().Handle(
                new GetNotificationConfigByIdQuery { Id = 99 },
                CancellationToken.None);

            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("*99*not found*");
        }

        [Fact]
        public async Task Handle_ValidId_PublishesAuditEvent()
        {
            var dto = new NotificationConfigDto { Id = 1, ModuleName = "Test" };

            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);
            _mockMapper.Setup(m => m.Map<NotificationConfigDto>(It.IsAny<NotificationConfigDto>())).Returns(dto);

            await CreateSut().Handle(new GetNotificationConfigByIdQuery { Id = 1 }, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
