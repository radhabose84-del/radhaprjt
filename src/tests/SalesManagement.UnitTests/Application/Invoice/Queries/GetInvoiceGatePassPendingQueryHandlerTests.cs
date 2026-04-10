using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IInvoice;
using SalesManagement.Application.Invoice.Queries.GetInvoiceGatePassPending;

namespace SalesManagement.UnitTests.Application.Invoice.Queries
{
    public sealed class GetInvoiceGatePassPendingQueryHandlerTests
    {
        private readonly Mock<IInvoiceQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IPartyLookup> _mockPartyLookup = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
        private readonly Mock<IItemLookup> _mockItemLookup = new(MockBehavior.Loose);
        private readonly Mock<IUOMLookup> _mockUomLookup = new(MockBehavior.Loose);
        private readonly Mock<IFinancialYearLookup> _mockFinYearLookup = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetInvoiceGatePassPendingQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockPartyLookup.Object, _mockUnitLookup.Object,
                _mockItemLookup.Object, _mockUomLookup.Object, _mockFinYearLookup.Object,
                _mockMediator.Object);

        [Fact]
        public async Task Handle_NoPending_ReturnsEmptyList()
        {
            _mockRepo
                .Setup(r => r.GetInvoiceGatePassPendingAsync(It.IsAny<string?>()))
                .ReturnsAsync(new List<GetInvoiceGatePassPendingDto>());

            var result = await CreateSut().Handle(
                new GetInvoiceGatePassPendingQuery { VehicleNo = "KA01" }, CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
