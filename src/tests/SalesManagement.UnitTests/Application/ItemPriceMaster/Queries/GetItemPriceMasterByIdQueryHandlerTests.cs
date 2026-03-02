using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IItemPriceMaster;
using SalesManagement.Application.ItemPriceMaster.Dto;
using SalesManagement.Application.ItemPriceMaster.Queries.GetItemPriceMasterById;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.ItemPriceMaster.Queries
{
    public class GetItemPriceMasterByIdQueryHandlerTests
    {
        private readonly Mock<IItemPriceMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new();
        private readonly Mock<IMediator> _mockMediator = new();

        private GetItemPriceMasterByIdQueryHandler CreateSut()
        {
            _mockMapper.Setup(m => m.Map<ItemPriceMasterDto>(It.IsAny<object>()))
                .Returns<object>(o => (o as ItemPriceMasterDto)!);
            _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            return new GetItemPriceMasterByIdQueryHandler(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
        }

        [Fact]
        public async Task Handle_EntityFound_ReturnsSuccessWithData()
        {
            var dto = ItemPriceMasterBuilders.ValidDto(id: 5);
            _mockQueryRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(dto);

            var result = await CreateSut().Handle(
                new GetItemPriceMasterByIdQuery { Id = 5 }, CancellationToken.None);

            result.Should().NotBeNull();
            result.Should().Be(dto);
        }

        [Fact]
        public async Task Handle_EntityFound_ReturnsCorrectDto()
        {
            var dto = ItemPriceMasterBuilders.ValidDto(id: 7, priceCode: "PC777");
            _mockQueryRepo.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(dto);

            var result = await CreateSut().Handle(
                new GetItemPriceMasterByIdQuery { Id = 7 }, CancellationToken.None);

            result!.PriceCode.Should().Be("PC777");
        }

        [Fact]
        public async Task Handle_EntityNotFound_ReturnsNull()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((ItemPriceMasterDto?)null);

            var result = await CreateSut().Handle(
                new GetItemPriceMasterByIdQuery { Id = 99 }, CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_CallsGetByIdAsync_WithCorrectId()
        {
            var dto = ItemPriceMasterBuilders.ValidDto(id: 3);
            _mockQueryRepo.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(dto);

            await CreateSut().Handle(
                new GetItemPriceMasterByIdQuery { Id = 3 }, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetByIdAsync(3), Times.Once);
        }
    }
}
