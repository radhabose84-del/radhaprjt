using AutoMapper;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationTemplate;
using BackgroundService.Application.Notification.NotificationTemplate.Queries.GetAllNotificationTemplate;
using BackgroundService.Application.Notification.NotificationTemplate.Queries.GetNotificationTemplateAutoComplete;
using BackgroundService.Domain.Events;
using MediatR;

namespace BackgroundService.UnitTests.Application.Notification.NotificationTemplate.Queries
{
    public sealed class GetNotificationTemplateAutoCompleteQueryHandlerTests
    {
        private readonly Mock<INotificationTemplateQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private GetNotificationTemplateAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_ReturnsResults()
        {
            var list = new List<NotificationTemplateAutoCompleteDto>
            {
                new() { Id = 1, ModuleName = "Test" }
            };

            _mockQueryRepo
                .Setup(r => r.GetNotificationTemplateAutoCompleteAsync("Test"))
                .ReturnsAsync(list);

            _mockMapper
                .Setup(m => m.Map<List<NotificationTemplateAutoCompleteDto>>(It.IsAny<List<NotificationTemplateAutoCompleteDto>>()))
                .Returns(list);

            var result = await CreateSut().Handle(
                new GetNotificationTemplateAutoCompleteQuery { SearchPattern = "Test" },
                CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_NullSearch_UsesEmptyString()
        {
            _mockQueryRepo
                .Setup(r => r.GetNotificationTemplateAutoCompleteAsync(string.Empty))
                .ReturnsAsync(new List<NotificationTemplateAutoCompleteDto>());

            _mockMapper
                .Setup(m => m.Map<List<NotificationTemplateAutoCompleteDto>>(It.IsAny<List<NotificationTemplateAutoCompleteDto>>()))
                .Returns(new List<NotificationTemplateAutoCompleteDto>());

            var result = await CreateSut().Handle(
                new GetNotificationTemplateAutoCompleteQuery { SearchPattern = null },
                CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
