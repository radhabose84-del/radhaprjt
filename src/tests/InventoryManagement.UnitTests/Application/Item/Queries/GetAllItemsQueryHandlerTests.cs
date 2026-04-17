using InventoryManagement.Application.Common.Interfaces.Item.ItemDetail.Queries;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems;

namespace InventoryManagement.UnitTests.Application.Item.Queries
{
    public sealed class GetAllItemsQueryHandlerTests
    {
        private readonly Mock<IItemQueryRepository> _mockRepo = new(MockBehavior.Strict);

        private GetAllItemsQueryHandler CreateSut() => new(_mockRepo.Object);

        [Fact]
        public async Task Handle_ReturnsTupleWithItemsAndCount()
        {
            var items = new List<ItemListDto> { new ItemListDto { Id = 1 } };
            _mockRepo.Setup(r => r.GetAllAsync(
                    It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<string?>(),
                    It.IsAny<bool>(), It.IsAny<int?>(), It.IsAny<int?>(),
                    It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((items, 1));

            var result = await CreateSut().Handle(new GetAllItemsQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.Items.Should().HaveCount(1);
            result.TotalCount.Should().Be(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyItems()
        {
            _mockRepo.Setup(r => r.GetAllAsync(
                    It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<string?>(),
                    It.IsAny<bool>(), It.IsAny<int?>(), It.IsAny<int?>(),
                    It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((new List<ItemListDto>(), 0));

            var result = await CreateSut().Handle(new GetAllItemsQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.Items.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task Handle_CallsGetAllOnce()
        {
            _mockRepo.Setup(r => r.GetAllAsync(
                    It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<string?>(),
                    It.IsAny<bool>(), It.IsAny<int?>(), It.IsAny<int?>(),
                    It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((new List<ItemListDto>(), 0));

            await CreateSut().Handle(new GetAllItemsQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            _mockRepo.Verify(r => r.GetAllAsync(
                It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<string?>(),
                It.IsAny<bool>(), It.IsAny<int?>(), It.IsAny<int?>(),
                It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
