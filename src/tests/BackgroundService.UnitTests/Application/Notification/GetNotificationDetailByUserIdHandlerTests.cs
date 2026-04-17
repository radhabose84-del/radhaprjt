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
            _mockRepo.Setup(r => r.GetAllByUserIdAsync("user1")).ReturnsAsync(entities);

            var result = await CreateSut().Handle(
                new GetNotificationDetailByUserId { UserId = "user1" },
                CancellationToken.None);

            result.Should().HaveCount(2);
            result[0].Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NullRepoResult_ReturnsEmptyList()
        {
            _mockRepo.Setup(r => r.GetAllByUserIdAsync(It.IsAny<string>()))
                .ReturnsAsync((List<GetNotificationDetailDto>?)null!);

            var result = await CreateSut().Handle(
                new GetNotificationDetailByUserId { UserId = "nobody" },
                CancellationToken.None);

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_TrimsUserId_BeforeQuery()
        {
            _mockRepo.Setup(r => r.GetAllByUserIdAsync("abc"))
                .ReturnsAsync(new List<GetNotificationDetailDto>());

            await CreateSut().Handle(
                new GetNotificationDetailByUserId { UserId = "  abc  " },
                CancellationToken.None);

            _mockRepo.Verify(r => r.GetAllByUserIdAsync("abc"), Times.Once);
        }

        [Fact]
        public async Task Handle_NullUserId_UsesEmptyString()
        {
            _mockRepo.Setup(r => r.GetAllByUserIdAsync(string.Empty))
                .ReturnsAsync(new List<GetNotificationDetailDto>());

            var result = await CreateSut().Handle(
                new GetNotificationDetailByUserId { UserId = null },
                CancellationToken.None);

            result.Should().BeEmpty();
            _mockRepo.Verify(r => r.GetAllByUserIdAsync(string.Empty), Times.Once);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            _mockRepo.Setup(r => r.GetAllByUserIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<GetNotificationDetailDto>
                {
                    new() { Id = 1 }
                });

            await CreateSut().Handle(
                new GetNotificationDetailByUserId { UserId = "user1" },
                CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e => e.Module == "NotificationDetail" && e.ActionDetail == "GetAllByUserId"),
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
