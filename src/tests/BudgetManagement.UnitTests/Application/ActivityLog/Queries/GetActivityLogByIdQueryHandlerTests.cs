using BudgetManagement.Domain.Entities;

namespace BudgetManagement.UnitTests.Application.ActivityLog.Queries
{
    public sealed class GetActivityLogByIdQueryHandlerTests
    {
        private readonly Mock<IActivityLogQueryRepository> _mockRepo = new(MockBehavior.Strict);

        private GetActivityLogByIdQueryHandler CreateSut() => new(_mockRepo.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsLog()
        {
            var log = new BudgetManagement.Domain.Entities.ActivityLog { Id = 1 };
            _mockRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(log);

            var result = await CreateSut().Handle(new GetActivityLogByIdQuery(1), CancellationToken.None);
            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NotFound_ReturnsNull()
        {
            _mockRepo.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((BudgetManagement.Domain.Entities.ActivityLog?)null);

            var result = await CreateSut().Handle(new GetActivityLogByIdQuery(99), CancellationToken.None);
            result.Should().BeNull();
        }
    }
}
