using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Production;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Warehouse;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDeliveryChallan;
using SalesManagement.Application.DeliveryChallan.Queries.GetDCGatePassPending;

namespace SalesManagement.UnitTests.Application.DeliveryChallan.Queries
{
    public sealed class GetDCGatePassPendingQueryHandlerTests
    {
        private readonly Mock<IDeliveryChallanQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
        private readonly Mock<IWarehouseLookup> _mockWarehouseLookup = new(MockBehavior.Loose);
        private readonly Mock<IPartyLookup> _mockPartyLookup = new(MockBehavior.Loose);
        private readonly Mock<IItemLookup> _mockItemLookup = new(MockBehavior.Loose);
        private readonly Mock<IUOMLookup> _mockUomLookup = new(MockBehavior.Loose);
        private readonly Mock<ILotMasterLookup> _mockLotLookup = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetDCGatePassPendingQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockUnitLookup.Object, _mockWarehouseLookup.Object,
                _mockPartyLookup.Object, _mockItemLookup.Object, _mockUomLookup.Object,
                _mockLotLookup.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_NoPending_ReturnsEmptyList()
        {
            _mockRepo
                .Setup(r => r.GetDCGatePassPendingAsync(It.IsAny<string?>()))
                .ReturnsAsync(new List<GetDCGatePassPendingDto>());

            var result = await CreateSut().Handle(
                new GetDCGatePassPendingQuery { VehicleNo = "KA01" }, CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
