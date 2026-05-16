using InventoryManagement.Application.Common.Interfaces.Item.ItemCategory;
using InventoryManagement.Application.Item.ItemCategory.Queries.GetItemCategorySampleQuantity;
using InventoryManagement.Application.Item.ItemCategory.Queries.Shared;

namespace InventoryManagement.UnitTests.Application.ItemCategory.Queries
{
    public sealed class GetItemCategorySampleQuantityQueryHandlerTests
    {
        private readonly Mock<IItemCategoryQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private GetItemCategorySampleQuantityQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Handle_FoundConfig_ReturnsDto()
        {
            var dto = new SampleQuantityDto
            {
                Id = 7,
                UnitId = 2,
                UnitName = "Unit-2",
                UOMId = 1,
                UOMName = "KG",
                MaxSampleQuantity = 5.5m,
                IsActive = 1
            };

            _mockQueryRepo
                .Setup(r => r.GetSampleQuantityAsync(10, 2, 1))
                .ReturnsAsync(dto);

            var result = await CreateSut().Handle(
                new GetItemCategorySampleQuantityQuery { ItemCategoryId = 10, UnitId = 2, UOMId = 1 },
                CancellationToken.None);

            result.Should().NotBeNull();
            result!.Id.Should().Be(7);
            result.MaxSampleQuantity.Should().Be(5.5m);
        }

        [Fact]
        public async Task Handle_NoConfig_ReturnsNull()
        {
            _mockQueryRepo
                .Setup(r => r.GetSampleQuantityAsync(10, 99, 5))
                .ReturnsAsync((SampleQuantityDto?)null);

            var result = await CreateSut().Handle(
                new GetItemCategorySampleQuantityQuery { ItemCategoryId = 10, UnitId = 99, UOMId = 5 },
                CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_CallsRepositoryOnce_WithUomId()
        {
            _mockQueryRepo
                .Setup(r => r.GetSampleQuantityAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync((SampleQuantityDto?)null);

            await CreateSut().Handle(
                new GetItemCategorySampleQuantityQuery { ItemCategoryId = 1, UnitId = 1, UOMId = 3 },
                CancellationToken.None);

            _mockQueryRepo.Verify(
                r => r.GetSampleQuantityAsync(1, 1, 3),
                Times.Once);
        }
    }
}
