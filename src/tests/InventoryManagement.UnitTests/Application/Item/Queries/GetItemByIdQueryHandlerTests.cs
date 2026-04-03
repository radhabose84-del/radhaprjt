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
    }
}
