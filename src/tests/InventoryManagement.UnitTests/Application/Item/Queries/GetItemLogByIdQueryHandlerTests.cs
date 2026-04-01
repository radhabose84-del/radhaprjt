using InventoryManagement.Application.Item.ItemDetail.Queries.GetItemLogById;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetItemLogs;

namespace InventoryManagement.UnitTests.Application.Item.Queries
{
    public sealed class GetItemLogByIdQueryHandlerTests
    {
        private readonly Mock<IItemLogQueryRepository> _mockRepo = new(MockBehavior.Strict);

        private GetItemLogByIdQueryHandler CreateSut() => new(_mockRepo.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            var dto = new ItemLogDto { Id = 1 };
            _mockRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            var result = await CreateSut().Handle(new GetItemLogByIdQuery(1), CancellationToken.None);

            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NonExistentId_ReturnsNull()
        {
            _mockRepo.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ItemLogDto?)null);

            var result = await CreateSut().Handle(new GetItemLogByIdQuery(99), CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_CallsRepoOnce()
        {
            _mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ItemLogDto?)null);

            await CreateSut().Handle(new GetItemLogByIdQuery(5), CancellationToken.None);

            _mockRepo.Verify(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
