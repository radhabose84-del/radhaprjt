using AutoMapper;
using InventoryManagement.Application.Common.Interfaces.Item.ItemCategory;
using InventoryManagement.Application.Item.ItemCategory.Queries.GetItemCategory;
using InventoryManagement.Application.Item.ItemCategory.Queries.GetItemCategoryById;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.UnitTests.Application.ItemCategory.Queries
{
    public sealed class GetItemCategoryByIdQueryHandlerTests
    {
        private readonly Mock<IItemCategoryQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private GetItemCategoryByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            var dto = new ItemCategoryDto { Id = 1, ItemCategoryName = "Test Category" };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);
            _mockMapper.Setup(m => m.Map<ItemCategoryDto>(dto)).Returns(dto);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetItemCategoryByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NonExistentId_ThrowsKeyNotFoundException()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((ItemCategoryDto?)null);

            var act = async () => await CreateSut().Handle(new GetItemCategoryByIdQuery { Id = 99 }, CancellationToken.None);
            await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("*99*");
        }

        [Fact]
        public async Task Handle_ExistingId_CallsGetByIdOnce()
        {
            var dto = new ItemCategoryDto { Id = 1 };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);
            _mockMapper.Setup(m => m.Map<ItemCategoryDto>(dto)).Returns(dto);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(new GetItemCategoryByIdQuery { Id = 1 }, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetByIdAsync(1), Times.Once);
        }
    }
}
