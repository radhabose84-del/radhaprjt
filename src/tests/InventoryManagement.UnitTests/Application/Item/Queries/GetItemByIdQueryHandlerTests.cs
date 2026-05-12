using InventoryManagement.Application.Common.Interfaces.Item.ItemDetail.Queries;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetItemById;

namespace InventoryManagement.UnitTests.Application.Item.Queries
{
    public sealed class GetItemByIdQueryHandlerTests
    {
        private readonly Mock<IItemQueryRepository> _mockRepo = new(MockBehavior.Strict);

        private GetItemByIdQueryHandler CreateSut() => new(_mockRepo.Object);

        [Fact]
        public async Task Handle_ExistingItem_ReturnsDto()
        {
            var dto = new ItemDetailsDto { Id = 1 };
            _mockRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            var result = await CreateSut().Handle(new GetItemByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NonExistentItem_ReturnsNull()
        {
            _mockRepo.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ItemDetailsDto?)null);

            var result = await CreateSut().Handle(new GetItemByIdQuery { Id = 99 }, CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_CallsGetByIdOnce()
        {
            _mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ItemDetailsDto?)null);

            await CreateSut().Handle(new GetItemByIdQuery { Id = 5 }, CancellationToken.None);

            _mockRepo.Verify(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()), Times.Once);
        }

        // Regression for Bug #1 — IsCapitalItem was being dropped by the GetByIdAsync
        // LINQ projection in ItemQueryRepository. This handler test asserts the handler
        // returns whatever the repo returns; the integration test covers the projection
        // itself end-to-end.
        [Fact]
        public async Task Handle_WhenRepoReturnsIsCapitalItemTrue_HandlerPropagatesIt()
        {
            var dto = new ItemDetailsDto { Id = 7, IsCapitalItem = true };
            _mockRepo.Setup(r => r.GetByIdAsync(7, It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            var result = await CreateSut().Handle(new GetItemByIdQuery { Id = 7 }, CancellationToken.None);

            result.Should().NotBeNull();
            result!.IsCapitalItem.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_WhenRepoReturnsIsCapitalItemFalse_HandlerPropagatesIt()
        {
            var dto = new ItemDetailsDto { Id = 8, IsCapitalItem = false };
            _mockRepo.Setup(r => r.GetByIdAsync(8, It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            var result = await CreateSut().Handle(new GetItemByIdQuery { Id = 8 }, CancellationToken.None);

            result.Should().NotBeNull();
            result!.IsCapitalItem.Should().BeFalse();
        }
    }
}
