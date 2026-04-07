using Contracts.Dtos.Lookups.Inventory;
using Contracts.Dtos.Lookups.Party;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using PurchaseManagement.Application.Common;
using PurchaseManagement.Application.Common.Interfaces.PriceMaster;
using PurchaseManagement.Application.PriceMaster.Dtos;
using PurchaseManagement.Application.PriceMaster.Queries.GetAll;

namespace PurchaseManagement.UnitTests.Application.PriceMaster.Queries
{
    public sealed class GetAllPriceMasterQueryHandlerTests
    {
        private readonly Mock<IPriceMasterQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IItemLookup> _mockItemLookup = new(MockBehavior.Loose);
        private readonly Mock<IUOMLookup> _mockUomLookup = new(MockBehavior.Loose);
        private readonly Mock<IPartyLookup> _mockPartyLookup = new(MockBehavior.Loose);
        private readonly Mock<ICurrencyLookup> _mockCurrencyLookup = new(MockBehavior.Loose);

        private GetAllPriceMasterQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockItemLookup.Object, _mockUomLookup.Object,
                _mockPartyLookup.Object, _mockCurrencyLookup.Object);

        private static PagedResult<PriceMasterGetAllDto> EmptyPage() =>
            new() { Items = Array.Empty<PriceMasterGetAllDto>(), Total = 0, Page = 1, PageSize = 15 };

        private static PagedResult<PriceMasterGetAllDto> SingleItemPage(int itemId = 1, int vendorId = 2, int uomId = 3)
        {
            var dto = new PriceMasterGetAllDto
            {
                Id = 1,
                ItemId = itemId,
                VendorId = vendorId,
                UomId = uomId,
                ValidFrom = new DateOnly(2025, 1, 1),
                StatusId = 1,
                SourceFromId = 1,
                IsActive = 1,
                Details = new List<PriceMasterDetailUpsertDto>
                {
                    new() { ScaleQtyFrom = 1, UnitPrice = 100m, CurrencyId = 5 }
                }
            };
            return new PagedResult<PriceMasterGetAllDto>
            {
                Items = new List<PriceMasterGetAllDto> { dto },
                Total = 1,
                Page = 1,
                PageSize = 15
            };
        }

        private void SetupRepoGetAll(PagedResult<PriceMasterGetAllDto> result)
        {
            _mockRepo
                .Setup(r => r.GetAllAsync(
                    It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<string?>(),
                    It.IsAny<int?>(), It.IsAny<decimal?>(), It.IsAny<decimal?>(),
                    It.IsAny<int?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);
        }

        private void SetupLookups(
            int itemId = 1, string itemCode = "ITM001", string itemName = "Test Item",
            int vendorId = 2, string vendorCode = "V001", string vendorName = "Test Vendor",
            int uomId = 3, string uomCode = "EA", string uomName = "Each",
            int currencyId = 5, string currencyCode = "USD")
        {
            _mockItemLookup
                .Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ItemLookupDto>
                {
                    new() { Id = itemId, ItemCode = itemCode, ItemName = itemName }
                });

            _mockUomLookup
                .Setup(l => l.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UOMLookupDto>
                {
                    new() { Id = uomId, Code = uomCode, UOMName = uomName }
                });

            _mockPartyLookup
                .Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PartyLookupDto>
                {
                    new() { Id = vendorId, PartyCode = vendorCode, PartyName = vendorName }
                });

            _mockCurrencyLookup
                .Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CurrencyLookupDto>
                {
                    new() { CurrencyId = currencyId, Code = currencyCode }
                });
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyPage()
        {
            SetupRepoGetAll(EmptyPage());

            var query = new GetAllPriceMasterQuery { PageNumber = 1, PageSize = 15 };
            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Items.Should().BeEmpty();
            result.Total.Should().Be(0);
        }

        [Fact]
        public async Task Handle_WithData_ReturnsItems()
        {
            SetupRepoGetAll(SingleItemPage());
            SetupLookups();

            var query = new GetAllPriceMasterQuery { PageNumber = 1, PageSize = 15 };
            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Items.Should().HaveCount(1);
            result.Total.Should().Be(1);
        }

        [Fact]
        public async Task Handle_EnrichesItemData()
        {
            SetupRepoGetAll(SingleItemPage(itemId: 1));
            SetupLookups(itemId: 1, itemCode: "ITM001", itemName: "Widget");

            var result = await CreateSut().Handle(
                new GetAllPriceMasterQuery { PageNumber = 1, PageSize = 15 },
                CancellationToken.None);

            result.Items.First().ItemCode.Should().Be("ITM001");
            result.Items.First().ItemName.Should().Be("Widget");
        }

        [Fact]
        public async Task Handle_EnrichesVendorData()
        {
            SetupRepoGetAll(SingleItemPage(vendorId: 2));
            SetupLookups(vendorId: 2, vendorCode: "V100", vendorName: "Acme Corp");

            var result = await CreateSut().Handle(
                new GetAllPriceMasterQuery { PageNumber = 1, PageSize = 15 },
                CancellationToken.None);

            result.Items.First().VendorCode.Should().Be("V100");
            result.Items.First().VendorName.Should().Be("Acme Corp");
        }

        [Fact]
        public async Task Handle_EnrichesUomData()
        {
            SetupRepoGetAll(SingleItemPage(uomId: 3));
            SetupLookups(uomId: 3, uomCode: "KG", uomName: "Kilogram");

            var result = await CreateSut().Handle(
                new GetAllPriceMasterQuery { PageNumber = 1, PageSize = 15 },
                CancellationToken.None);

            result.Items.First().UOM.Should().Be("Kilogram");
        }

        [Fact]
        public async Task Handle_EnrichesCurrencyOnDetails()
        {
            SetupRepoGetAll(SingleItemPage());
            SetupLookups(currencyId: 5, currencyCode: "EUR");

            var result = await CreateSut().Handle(
                new GetAllPriceMasterQuery { PageNumber = 1, PageSize = 15 },
                CancellationToken.None);

            var detail = result.Items.First().Details.First();
            detail.CurrencyName.Should().Be("EUR");
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var page = SingleItemPage();
            SetupRepoGetAll(page);
            SetupLookups();

            var result = await CreateSut().Handle(
                new GetAllPriceMasterQuery { PageNumber = 1, PageSize = 15 },
                CancellationToken.None);

            result.Page.Should().Be(1);
            result.PageSize.Should().Be(15);
        }

        [Fact]
        public async Task Handle_WithSearchTerm_FiltersResults()
        {
            var dto1 = new PriceMasterGetAllDto
            {
                Id = 1, ItemId = 1, VendorId = 2, UomId = 3,
                ValidFrom = new DateOnly(2025, 1, 1), StatusId = 1, SourceFromId = 1, IsActive = 1,
                Details = new List<PriceMasterDetailUpsertDto>()
            };
            var dto2 = new PriceMasterGetAllDto
            {
                Id = 2, ItemId = 4, VendorId = 5, UomId = 3,
                ValidFrom = new DateOnly(2025, 1, 1), StatusId = 1, SourceFromId = 1, IsActive = 1,
                Details = new List<PriceMasterDetailUpsertDto>()
            };
            var page = new PagedResult<PriceMasterGetAllDto>
            {
                Items = new List<PriceMasterGetAllDto> { dto1, dto2 },
                Total = 2, Page = 1, PageSize = 15
            };
            SetupRepoGetAll(page);

            _mockItemLookup
                .Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ItemLookupDto>
                {
                    new() { Id = 1, ItemCode = "WIDGET01", ItemName = "Widget" },
                    new() { Id = 4, ItemCode = "BOLT01", ItemName = "Bolt" }
                });

            _mockUomLookup
                .Setup(l => l.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UOMLookupDto> { new() { Id = 3, Code = "EA", UOMName = "Each" } });

            _mockPartyLookup
                .Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PartyLookupDto>
                {
                    new() { Id = 2, PartyCode = "V001", PartyName = "Vendor A" },
                    new() { Id = 5, PartyCode = "V002", PartyName = "Vendor B" }
                });

            _mockCurrencyLookup
                .Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CurrencyLookupDto>());

            var result = await CreateSut().Handle(
                new GetAllPriceMasterQuery { PageNumber = 1, PageSize = 15, SearchTerm = "Widget" },
                CancellationToken.None);

            // Only the item with "Widget" in ItemCode/ItemName should remain
            result.Items.Should().HaveCount(1);
            result.Items.First().ItemCode.Should().Be("WIDGET01");
        }

        [Fact]
        public async Task Handle_CallsAllLookups()
        {
            SetupRepoGetAll(SingleItemPage());
            SetupLookups();

            await CreateSut().Handle(
                new GetAllPriceMasterQuery { PageNumber = 1, PageSize = 15 },
                CancellationToken.None);

            _mockItemLookup.Verify(
                l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()),
                Times.Once);
            _mockUomLookup.Verify(
                l => l.GetAllAsync(It.IsAny<CancellationToken>()),
                Times.Once);
            _mockPartyLookup.Verify(
                l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()),
                Times.Once);
            _mockCurrencyLookup.Verify(
                l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_EmptyResult_DoesNotCallLookups()
        {
            SetupRepoGetAll(EmptyPage());

            await CreateSut().Handle(
                new GetAllPriceMasterQuery { PageNumber = 1, PageSize = 15 },
                CancellationToken.None);

            _mockItemLookup.Verify(
                l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
