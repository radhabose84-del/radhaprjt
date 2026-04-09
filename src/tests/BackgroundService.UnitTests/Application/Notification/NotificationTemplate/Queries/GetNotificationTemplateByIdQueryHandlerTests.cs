using AutoMapper;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationTemplate;
using BackgroundService.Application.Notification.NotificationTemplate.Queries.GetAllNotificationTemplate;
using BackgroundService.Application.Notification.NotificationTemplate.Queries.GetNotificationTemplateById;
using BackgroundService.Domain.Events;
using MediatR;

namespace BackgroundService.UnitTests.Application.Notification.NotificationTemplate.Queries
{
    public sealed class GetNotificationTemplateByIdQueryHandlerTests
    {
        private readonly Mock<INotificationTemplateQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private GetNotificationTemplateByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_ValidId_ReturnsDto()
        {
            var dto = new NotificationTemplateDto { Id = 1, SubjectTemplate = "Subject" };

            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);
            _mockMapper.Setup(m => m.Map<NotificationTemplateDto>(It.IsAny<NotificationTemplateDto>())).Returns(dto);

            var result = await CreateSut().Handle(
                new GetNotificationTemplateByIdQuery { Id = 1 },
                CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NotFound_ThrowsKeyNotFoundException()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((NotificationTemplateDto?)null);

            Func<Task> act = async () => await CreateSut().Handle(
                new GetNotificationTemplateByIdQuery { Id = 99 },
                CancellationToken.None);

            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("*99*not found*");
        }
    }
}
