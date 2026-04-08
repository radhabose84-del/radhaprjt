using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ImportPO;
using PurchaseManagement.Application.PurchaseOrder.Dtos.ImportPO;
using PurchaseManagement.Application.PurchaseOrder.ImportPO.Queries.GetPOById;

namespace PurchaseManagement.UnitTests.Application.PurchaseOrder.ImportPO.Queries
{
    public sealed class GetImportPOByIdQueryHandlerTests
    {
        private readonly Mock<IImportPOQueryRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IPartyLookup> _mockPartyLookup = new(MockBehavior.Loose);
        private readonly Mock<ICurrencyLookup> _mockCurrencyLookup = new(MockBehavior.Loose);
        private readonly Mock<IUOMLookup> _mockUomLookup = new(MockBehavior.Loose);
        private readonly Mock<IItemLookup> _mockItemLookup = new(MockBehavior.Loose);
        private readonly Mock<ICompanyLookup> _mockCompanyLookup = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);

        private GetImportPOByIdQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockIp.Object, _mockPartyLookup.Object,
                _mockCurrencyLookup.Object, _mockUomLookup.Object, _mockItemLookup.Object,
                _mockCompanyLookup.Object, _mockUnitLookup.Object);

        [Fact]
        public async Task Handle_NotFound_ReturnsNull()
        {
            _mockRepo
                .Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ImportPOFullVm?)null);

            var result = await CreateSut().Handle(new GetImportPOByIdQuery(99), CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            // Provide a fully-initialized VM with a non-null PO header,
            // because the handler accesses vm.PO.VendorId without null guard.
            _mockRepo
                .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ImportPOFullVm
                {
                    PO = new PurchaseOrderHeaderSummaryDto { Id = 1, VendorId = 0, CurrencyId = 0 }
                });

            var result = await CreateSut().Handle(new GetImportPOByIdQuery(1), CancellationToken.None);

            result.Should().NotBeNull();
        }
    }
}
