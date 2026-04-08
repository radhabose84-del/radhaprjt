using AutoMapper;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationTemplate;
using BackgroundService.Application.Notification.NotificationTemplate.Queries.GetAllNotificationTemplate;
using BackgroundService.Domain.Events;
using MediatR;

namespace BackgroundService.UnitTests.Application.Notification.NotificationTemplate.Queries
{
    public sealed class GetAllNotificationTemplateQueryHandlerTests
    {
        private readonly Mock<INotificationTemplateQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private GetAllNotificationTemplateQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var dtoList = new List<NotificationTemplateDto>
            {
                new() { Id = 1, SubjectTemplate = "Subject" }
            };
            var dynamicList = dtoList.Cast<dynamic>().ToList() as IEnumerable<dynamic>;

            _mockQueryRepo
                .Setup(r => r.GetAllNotificationTemplateAsync(1, 15, null))
                .ReturnsAsync((dynamicList, 1));

            _mockMapper
                .Setup(m => m.Map<List<NotificationTemplateDto>>(It.IsAny<IEnumerable<dynamic>>()))
                .Returns(dtoList);

            var result = await CreateSut().Handle(
                new GetAllNotificationTemplateQuery { PageNumber = 1, PageSize = 15 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            var emptyList = Enumerable.Empty<dynamic>();

            _mockQueryRepo
                .Setup(r => r.GetAllNotificationTemplateAsync(1, 15, null))
                .ReturnsAsync((emptyList, 0));

            _mockMapper
                .Setup(m => m.Map<List<NotificationTemplateDto>>(It.IsAny<IEnumerable<dynamic>>()))
                .Returns(new List<NotificationTemplateDto>());

            var result = await CreateSut().Handle(
                new GetAllNotificationTemplateQuery { PageNumber = 1, PageSize = 15 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }
    }
}
