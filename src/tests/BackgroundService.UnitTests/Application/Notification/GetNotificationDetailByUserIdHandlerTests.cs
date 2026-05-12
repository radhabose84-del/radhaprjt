using AutoMapper;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationDetail;
using BackgroundService.Application.Notification.GetNotificationDetail.GetNotificationDetailById;
using BackgroundService.Domain.Events;
using MediatR;

namespace BackgroundService.UnitTests.Application.Notification
{
    public sealed class GetNotificationDetailByUserIdHandlerTests
    {
        private readonly Mock<INotificationDetailRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private GetNotificationDetailByUserIdHandler CreateSut() =>
            new(_mockRepo.Object, _mockMediator.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_ReturnsMapped_Notifications()
        {
            var entities = new List<GetNotificationDetailDto>
            {
                new() { Id = 1, ModuleName = "M1" },
                new() { Id = 2, ModuleName = "M2" }
            };
            _mockRepo.Setup(r => r.GetAllByUserIdAsync(
                "user1", 1, 10,
                It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(), It.IsAny<string?>()))
                .ReturnsAsync((entities, entities.Count));

            var result = await CreateSut().Handle(
                new GetNotificationDetailByUserId { UserId = "user1", PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.Data.Should().HaveCount(2);
            result.Data![0].Id.Should().Be(1);
            result.TotalCount.Should().Be(2);
        }

        [Fact]
        public async Task Handle_EmptyRepoResult_ReturnsEmptyList()
        {
            _mockRepo.Setup(r => r.GetAllByUserIdAsync(
                It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(), It.IsAny<string?>()))
                .ReturnsAsync((new List<GetNotificationDetailDto>(), 0));

            var result = await CreateSut().Handle(
                new GetNotificationDetailByUserId { UserId = "nobody", PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.Should().NotBeNull();
            result.Data.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task Handle_TrimsUserId_BeforeQuery()
        {
            _mockRepo.Setup(r => r.GetAllByUserIdAsync(
                "abc", 1, 10,
                It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(), It.IsAny<string?>()))
                .ReturnsAsync((new List<GetNotificationDetailDto>(), 0));

            await CreateSut().Handle(
                new GetNotificationDetailByUserId { UserId = "  abc  ", PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            _mockRepo.Verify(r => r.GetAllByUserIdAsync(
                "abc", 1, 10,
                It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(), It.IsAny<string?>()), Times.Once);
        }

        [Fact]
        public async Task Handle_NullUserId_UsesEmptyString()
        {
            _mockRepo.Setup(r => r.GetAllByUserIdAsync(
                string.Empty, 1, 10,
                It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(), It.IsAny<string?>()))
                .ReturnsAsync((new List<GetNotificationDetailDto>(), 0));

            var result = await CreateSut().Handle(
                new GetNotificationDetailByUserId { UserId = null, PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.Data.Should().BeEmpty();
            _mockRepo.Verify(r => r.GetAllByUserIdAsync(
                string.Empty, 1, 10,
                It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(), It.IsAny<string?>()), Times.Once);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            _mockRepo.Setup(r => r.GetAllByUserIdAsync(
                It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(), It.IsAny<string?>()))
                .ReturnsAsync((new List<GetNotificationDetailDto> { new() { Id = 1 } }, 1));

            await CreateSut().Handle(
                new GetNotificationDetailByUserId { UserId = "user1", PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e => e.Module == "NotificationDetail" && e.ActionDetail == "GetAllByUserId"),
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
