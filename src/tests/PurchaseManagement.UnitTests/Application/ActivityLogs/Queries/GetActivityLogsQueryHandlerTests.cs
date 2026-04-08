using PurchaseManagement.Domain.Entities;

namespace PurchaseManagement.UnitTests.Application.ActivityLogs.Queries
{
    public sealed class GetActivityLogsQueryHandlerTests
    {
        private readonly Mock<IActivityLogQueryRepository> _mockRepo = new(MockBehavior.Loose);

        private GetActivityLogsQueryHandler CreateSut() => new(_mockRepo.Object);

        [Fact]
        public async Task Handle_ReturnsItemsAndTotal()
        {
            var logs = new List<ActivityLog>
            {
                new ActivityLog { Id = 1, EntityName = "PurchaseOrder", EntityId = 1, PropertyName = "Status" }
            };
            _mockRepo
                .Setup(r => r.GetAllAsync("PurchaseOrder", 1, 1, 50, It.IsAny<CancellationToken>()))
                .ReturnsAsync((logs, 1));

            var (items, total) = await CreateSut().Handle(
                new GetActivityLogsQuery("PurchaseOrder", 1, 1, 50), CancellationToken.None);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            _mockRepo
                .Setup(r => r.GetAllAsync("Unknown", 999, 1, 50, It.IsAny<CancellationToken>()))
                .ReturnsAsync((new List<ActivityLog>(), 0));

            var (items, total) = await CreateSut().Handle(
                new GetActivityLogsQuery("Unknown", 999, 1, 50), CancellationToken.None);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task Handle_CallsRepositoryOnce()
        {
            _mockRepo
                .Setup(r => r.GetAllAsync("PurchaseOrder", 1, 1, 50, It.IsAny<CancellationToken>()))
                .ReturnsAsync((new List<ActivityLog>(), 0));

            await CreateSut().Handle(
                new GetActivityLogsQuery("PurchaseOrder", 1, 1, 50), CancellationToken.None);

            _mockRepo.Verify(
                r => r.GetAllAsync("PurchaseOrder", 1, 1, 50, It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
