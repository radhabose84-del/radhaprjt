using Contracts.Dtos.Lookups.Warehouse;
using Contracts.Interfaces.Lookups.Warehouse;
using MediatR;
using PurchaseManagement.Application.MRS.Queries.GetParentWarehouse;

namespace PurchaseManagement.UnitTests.Application.MRS.Queries
{
    public sealed class GetParentWarehouseQueryHandlerTests
    {
        private readonly Mock<IWarehouseLookup> _mockWarehouseLookup = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetParentWarehouseQueryHandler CreateSut() =>
            new(_mockWarehouseLookup.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_WarehouseNotFound_ReturnsNotFoundDto()
        {
            _mockWarehouseLookup
                .Setup(w => w.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<WarehouseLookupDto>());

            var result = await CreateSut().Handle(
                new GetParentWarehouseQuery { WarehouseId = 999 }, CancellationToken.None);

            result.Should().NotBeNull();
            result.ParentWarehouseId.Should().Be(0);
            result.ParentWarehouseName.Should().Be("Warehouse Not Found");
        }

        [Fact]
        public void QueryClass_Properties_ShouldBeAssignable()
        {
            var query = new GetParentWarehouseQuery { WarehouseId = 10 };
            query.WarehouseId.Should().Be(10);
        }
    }
}
