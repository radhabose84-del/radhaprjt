using PurchaseManagement.Domain.Entities;

namespace PurchaseManagement.UnitTests.Application.ActivityLogs.Queries
{
    public sealed class GetActivityLogByIdQueryHandlerTests
    {
        private readonly Mock<IActivityLogQueryRepository> _mockRepo = new(MockBehavior.Strict);

        private GetActivityLogByIdQueryHandler CreateSut() => new(_mockRepo.Object);

        [Fact]
        public async Task Handle_WhenFound_ReturnsActivityLog()
        {
            var log = new ActivityLog { Id = 1, EntityName = "PurchaseOrder", EntityId = 1, PropertyName = "Status" };
            _mockRepo
                .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(log);

            var result = await CreateSut().Handle(new GetActivityLogByIdQuery(1), CancellationToken.None);

            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_WhenNotFound_ReturnsNull()
        {
            _mockRepo
                .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ActivityLog?)null);

            var result = await CreateSut().Handle(new GetActivityLogByIdQuery(999), CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_CallsRepositoryOnce()
        {
            _mockRepo
                .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ActivityLog { Id = 1, EntityName = "PO", EntityId = 1, PropertyName = "X" });

            await CreateSut().Handle(new GetActivityLogByIdQuery(1), CancellationToken.None);

            _mockRepo.Verify(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
