#nullable disable
using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesItemPriceMaster;
using SalesManagement.Application.SalesItemPriceMaster.Dto;
using SalesManagement.Application.SalesItemPriceMaster.Queries.GetSalesItemPriceMasterById;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.SalesItemPriceMaster.Queries
{
    public class GetSalesItemPriceMasterByIdQueryHandlerTests
    {
        private readonly Mock<ISalesItemPriceMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new();
        private readonly Mock<IMediator> _mockMediator = new();

        private GetSalesItemPriceMasterByIdQueryHandler CreateSut()
        {
            _mockMapper.Setup(m => m.Map<SalesItemPriceMasterDto>(It.IsAny<object>()))
                .Returns<object>(o => o as SalesItemPriceMasterDto);
            _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            return new GetSalesItemPriceMasterByIdQueryHandler(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
        }

        [Fact]
        public async Task Handle_EntityFound_ReturnsSuccessWithData()
        {
            var dto = SalesItemPriceMasterBuilders.ValidDto(id: 5);
            _mockQueryRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(dto);

            var result = await CreateSut().Handle(
                new GetSalesItemPriceMasterByIdQuery { Id = 5 }, CancellationToken.None);

            result.Should().NotBeNull();
            result.Should().Be(dto);
        }

        [Fact]
        public async Task Handle_EntityFound_ReturnsCorrectDto()
        {
            var dto = SalesItemPriceMasterBuilders.ValidDto(id: 7, priceCode: "PC777");
            _mockQueryRepo.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(dto);

            var result = await CreateSut().Handle(
                new GetSalesItemPriceMasterByIdQuery { Id = 7 }, CancellationToken.None);

            result.PriceCode.Should().Be("PC777");
        }

        [Fact]
        public async Task Handle_EntityNotFound_ReturnsNull()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((SalesItemPriceMasterDto)null);

            var result = await CreateSut().Handle(
                new GetSalesItemPriceMasterByIdQuery { Id = 99 }, CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_CallsGetByIdAsync_WithCorrectId()
        {
            var dto = SalesItemPriceMasterBuilders.ValidDto(id: 3);
            _mockQueryRepo.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(dto);

            await CreateSut().Handle(
                new GetSalesItemPriceMasterByIdQuery { Id = 3 }, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetByIdAsync(3), Times.Once);
        }
    }
}
