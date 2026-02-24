#nullable disable
using Contracts.Common;
using SalesManagement.Application.Common.Interfaces.ISalesItemPriceMaster;
using SalesManagement.Application.SalesItemPriceMaster.Queries.GetSalesItemPriceMasterById;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.SalesItemPriceMaster.Queries
{
    public class GetSalesItemPriceMasterByIdQueryHandlerTests
    {
        private readonly Mock<ISalesItemPriceMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private GetSalesItemPriceMasterByIdQueryHandler CreateSut() =>
            new GetSalesItemPriceMasterByIdQueryHandler(_mockQueryRepo.Object);

        [Fact]
        public async Task Handle_EntityFound_ReturnsSuccessWithData()
        {
            var dto = SalesItemPriceMasterBuilders.ValidDto(id: 5);
            _mockQueryRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(dto);

            var result = await CreateSut().Handle(
                new GetSalesItemPriceMasterByIdQuery { Id = 5 }, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().Be(dto);
        }

        [Fact]
        public async Task Handle_EntityFound_ReturnsCorrectDto()
        {
            var dto = SalesItemPriceMasterBuilders.ValidDto(id: 7, priceCode: "PC777");
            _mockQueryRepo.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(dto);

            var result = await CreateSut().Handle(
                new GetSalesItemPriceMasterByIdQuery { Id = 7 }, CancellationToken.None);

            result.Data.PriceCode.Should().Be("PC777");
        }

        [Fact]
        public async Task Handle_EntityNotFound_ThrowsEntityNotFoundException()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((SalesManagement.Application.SalesItemPriceMaster.Dto.SalesItemPriceMasterDto)null);

            var act = async () => await CreateSut().Handle(
                new GetSalesItemPriceMasterByIdQuery { Id = 99 }, CancellationToken.None);

            await act.Should().ThrowAsync<EntityNotFoundException>();
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
