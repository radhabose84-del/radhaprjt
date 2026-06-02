using Contracts.Dtos.Lookups.Budget;
using Contracts.Dtos.Lookups.Inventory;
using Contracts.Dtos.Lookups.Party;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Budget;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using PurchaseManagement.Application.Common;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.Local;
using PurchaseManagement.Application.PurchaseOrder.Dtos.Local;
using PurchaseManagement.Application.PurchaseOrder.Local.Queries.GetPurchaseOrderAnalysis;

namespace PurchaseManagement.UnitTests.Application.PurchaseOrder.Local.Queries
{
    public sealed class GetPurchaseOrderAnalysisQueryHandlerTests
    {
        private readonly Mock<IPurchaseOrderQueryRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IPartyLookup> _mockPartyLookup = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
        private readonly Mock<IBudgetGroupLookup> _mockBudgetGroupLookup = new(MockBehavior.Loose);
        private readonly Mock<IInventoryCategoryLookup> _mockCategoryLookup = new(MockBehavior.Loose);

        private GetPurchaseOrderAnalysisQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockIp.Object, _mockPartyLookup.Object,
                _mockUnitLookup.Object, _mockBudgetGroupLookup.Object, _mockCategoryLookup.Object);

        private static GetPurchaseOrderAnalysisQuery Query(
            int? statusId = null, DateTimeOffset? from = null, DateTimeOffset? to = null, bool? isAmendment = null) =>
            new(1, 20, null, null, statusId, from, to, isAmendment);

        private void SetupRepo(PagedResult<PurchaseOrderAnalysisListItemDto> result)
        {
            _mockRepo
                .Setup(r => r.GetAnalysisAsync(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(),
                    It.IsAny<int?>(), It.IsAny<int?>(),
                    It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(), It.IsAny<bool?>(),
                    It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);
        }

        private static PagedResult<PurchaseOrderAnalysisListItemDto> Paged(params PurchaseOrderAnalysisListItemDto[] items) =>
            new() { Page = 1, PageSize = 20, Total = items.Length, Items = items };

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyWithoutEnrichment()
        {
            _mockIp.Setup(x => x.GetPartyId()).Returns((int?)null);
            SetupRepo(Paged());

            var result = await CreateSut().Handle(Query(), CancellationToken.None);

            result.Items.Should().BeEmpty();
            _mockPartyLookup.Verify(
                p => p.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_EnrichesVendorUnitBudgetGroupAndCategoryNames()
        {
            _mockIp.Setup(x => x.GetPartyId()).Returns((int?)null);
            SetupRepo(Paged(new PurchaseOrderAnalysisListItemDto
            {
                Id = 10, PONumber = "PO/2025/LOC/0001", VendorId = 5, UnitId = 1,
                BudgetGroupId = 7, ItemCategoryId = 9
            }));

            _mockPartyLookup
                .Setup(p => p.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PartyLookupDto> { new() { Id = 5, PartyName = "Sri Lakshmi Textiles" } });
            _mockUnitLookup
                .Setup(u => u.GetAllUnitAsync())
                .ReturnsAsync(new List<UnitLookupDto> { new() { UnitId = 1, UnitName = "Unit - Coimbatore" } });
            _mockBudgetGroupLookup
                .Setup(b => b.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<BudgetGroupLookupDto> { new() { Id = 7, Name = "Yarn" } });
            _mockCategoryLookup
                .Setup(c => c.GetCategoryByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CategoryMasterDto> { new() { Id = 9, ItemCategoryName = "Raw Material" } });

            var result = await CreateSut().Handle(Query(), CancellationToken.None);

            var item = result.Items.Single();
            item.VendorName.Should().Be("Sri Lakshmi Textiles");
            item.UnitName.Should().Be("Unit - Coimbatore");
            item.BudgetGroupName.Should().Be("Yarn");
            item.ItemCategoryName.Should().Be("Raw Material");
        }

        [Fact]
        public async Task Handle_BuyerScope_PassesNullVendorToRepo()
        {
            _mockIp.Setup(x => x.GetPartyId()).Returns((int?)null);
            SetupRepo(Paged());

            await CreateSut().Handle(Query(), CancellationToken.None);

            _mockRepo.Verify(r => r.GetAnalysisAsync(
                1, 20, null, null, null, null, null, null,
                null, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_SupplierScope_PassesPartyIdToRepo()
        {
            _mockIp.Setup(x => x.GetPartyId()).Returns(42);
            SetupRepo(Paged());

            await CreateSut().Handle(Query(), CancellationToken.None);

            _mockRepo.Verify(r => r.GetAnalysisAsync(
                1, 20, null, null, null, null, null, null,
                42, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_AmendmentAndDateFilters_FlowThroughToRepo()
        {
            _mockIp.Setup(x => x.GetPartyId()).Returns((int?)null);
            SetupRepo(Paged());
            var from = new DateTimeOffset(2025, 4, 2, 0, 0, 0, TimeSpan.Zero);
            var to = new DateTimeOffset(2026, 6, 2, 0, 0, 0, TimeSpan.Zero);

            await CreateSut().Handle(Query(statusId: 3, from: from, to: to, isAmendment: true), CancellationToken.None);

            _mockRepo.Verify(r => r.GetAnalysisAsync(
                1, 20, null, null, 3, from, to, true,
                null, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
