using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.Local;
using PurchaseManagement.Application.PurchaseOrder.Dtos.Local;
using PurchaseManagement.Application.PurchaseOrder.Local.Queries.GetPurchaseOrderById;

namespace PurchaseManagement.UnitTests.Application.PurchaseOrder.Local.Queries
{
    public sealed class GetPurchaseOrderByIdQueryHandlerTests
    {
        private readonly Mock<IPurchaseOrderQueryRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IPartyLookup> _mockPartyLookup = new(MockBehavior.Loose);
        private readonly Mock<ICurrencyLookup> _mockCurrencyLookup = new(MockBehavior.Loose);
        private readonly Mock<IUOMLookup> _mockUomLookup = new(MockBehavior.Loose);
        private readonly Mock<IItemLookup> _mockItemLookup = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);
        private readonly Mock<ICompanyLookup> _mockCompanyLookup = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);

        private GetPurchaseOrderByIdQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockIp.Object, _mockPartyLookup.Object,
                _mockCurrencyLookup.Object, _mockUomLookup.Object, _mockItemLookup.Object,
                _mockDeptLookup.Object, _mockCompanyLookup.Object, _mockUnitLookup.Object);

        [Fact]
        public async Task Handle_NotFound_ReturnsNull()
        {
            _mockRepo
                .Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((PurchaseOrderDetailDto?)null);

            var result = await CreateSut().Handle(new GetPurchaseOrderByIdQuery(99), CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            _mockRepo
                .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PurchaseOrderDetailDto());

            var result = await CreateSut().Handle(new GetPurchaseOrderByIdQuery(1), CancellationToken.None);

            result.Should().NotBeNull();
        }
    }
}
