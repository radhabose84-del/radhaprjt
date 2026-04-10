using AutoMapper;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationConfig;
using BackgroundService.Application.Notification.NotificationConfig.Queries.GetAllNotificationConfig;
using BackgroundService.Application.Notification.NotificationConfig.Queries.GetNotificationConfigAutoComplete;
using BackgroundService.Domain.Events;
using MediatR;

namespace BackgroundService.UnitTests.Application.Notification.NotificationConfig.Queries
{
    public sealed class GetNotificationConfigAutoCompleteQueryHandlerTests
    {
        private readonly Mock<INotificationConfigQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private GetNotificationConfigAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_ReturnsResults()
        {
            var list = new List<NotificationConfigAutoCompleteDto>
            {
                new() { Id = 1, ModuleName = "Test" }
            };

            _mockQueryRepo
                .Setup(r => r.GetNotificationConfigAutoCompleteAsync("Test"))
                .ReturnsAsync(list);

            _mockMapper
                .Setup(m => m.Map<List<NotificationConfigAutoCompleteDto>>(It.IsAny<List<NotificationConfigAutoCompleteDto>>()))
                .Returns(list);

            var result = await CreateSut().Handle(
                new GetNotificationConfigAutoCompleteQuery { SearchPattern = "Test" },
                CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].ModuleName.Should().Be("Test");
        }

        [Fact]
        public async Task Handle_EmptySearch_UsesEmptyString()
        {
            var list = new List<NotificationConfigAutoCompleteDto>();

            _mockQueryRepo
                .Setup(r => r.GetNotificationConfigAutoCompleteAsync(string.Empty))
                .ReturnsAsync(list);

            _mockMapper
                .Setup(m => m.Map<List<NotificationConfigAutoCompleteDto>>(It.IsAny<List<NotificationConfigAutoCompleteDto>>()))
                .Returns(list);

            var result = await CreateSut().Handle(
                new GetNotificationConfigAutoCompleteQuery { SearchPattern = null },
                CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
