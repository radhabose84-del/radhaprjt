using InventoryManagement.Application.Item.ItemDetail.Queries.GetItemLogById;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetItemLogs;
using InventoryManagement.Application.ItemLogs.Queries;

namespace InventoryManagement.UnitTests.Application.Item.Queries
{
    public sealed class GetItemLogsQueryHandlerTests
    {
        private readonly Mock<IItemLogQueryRepository> _mockRepo = new(MockBehavior.Strict);

        private GetItemLogsQueryHandler CreateSut() => new(_mockRepo.Object);

        [Fact]
        public async Task Handle_ReturnsTupleWithItemsAndCount()
        {
            var items = new List<ItemLogDto> { new ItemLogDto { Id = 1 } };
            _mockRepo.Setup(r => r.GetAllAsync(It.IsAny<ItemLogFilter>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((items, 1));

            var result = await CreateSut().Handle(
                new GetItemLogsQuery(new ItemLogFilter { Page = 1, Size = 10 }), CancellationToken.None);

            result.Items.Should().HaveCount(1);
            result.TotalCount.Should().Be(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmpty()
        {
            _mockRepo.Setup(r => r.GetAllAsync(It.IsAny<ItemLogFilter>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((new List<ItemLogDto>(), 0));

            var result = await CreateSut().Handle(
                new GetItemLogsQuery(new ItemLogFilter()), CancellationToken.None);

            result.Items.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task Handle_CallsRepoOnce()
        {
            _mockRepo.Setup(r => r.GetAllAsync(It.IsAny<ItemLogFilter>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((new List<ItemLogDto>(), 0));

            await CreateSut().Handle(
                new GetItemLogsQuery(new ItemLogFilter { EntityId = 5 }), CancellationToken.None);

            _mockRepo.Verify(r => r.GetAllAsync(It.IsAny<ItemLogFilter>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
