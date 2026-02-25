#nullable disable
using Contracts.Interfaces.Lookups.Users;
using SalesManagement.Application.Common.Interfaces.ISalesSegment;
using SalesManagement.Application.SalesSegment.Dto;
using SalesManagement.Application.SalesSegment.Queries.GetAllSalesSegment;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.SalesSegment.Queries
{
    /// <summary>
    /// ⚠️ GetAll populates CurrencyName via ICurrencyLookup for items that have a CurrencyId.
    /// When data is empty or no items have CurrencyId, GetByIdsAsync is NOT called.
    /// </summary>
    public class GetAllSalesSegmentQueryHandlerTests
    {
        private readonly Mock<ISalesSegmentQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<ICurrencyLookup> _mockCurrencyLookup = new(MockBehavior.Strict);

        private GetAllSalesSegmentQueryHandler CreateSut() =>
            new GetAllSalesSegmentQueryHandler(_mockQueryRepo.Object, _mockCurrencyLookup.Object);

        // ── Tests ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_ReturnsSuccess_WhenDataFound()
        {
            var data = new List<SalesSegmentDto>
            {
                SalesSegmentBuilders.ValidDto(id: 1),
                SalesSegmentBuilders.ValidDto(id: 2)
            };
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null)).ReturnsAsync((data, 2));

            var query = new GetAllSalesSegmentQuery { PageNumber = 1, PageSize = 10, SearchTerm = null };
            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ReturnsCorrectData()
        {
            var data = new List<SalesSegmentDto>
            {
                SalesSegmentBuilders.ValidDto(id: 1, segmentName: "Seg Alpha"),
                SalesSegmentBuilders.ValidDto(id: 2, segmentName: "Seg Beta")
            };
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null)).ReturnsAsync((data, 2));

            var query = new GetAllSalesSegmentQuery { PageNumber = 1, PageSize = 10, SearchTerm = null };
            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Data.Should().HaveCount(2);
            result.Data[0].SegmentName.Should().Be("Seg Alpha");
            result.Data[1].SegmentName.Should().Be("Seg Beta");
        }

        [Fact]
        public async Task Handle_ReturnsCorrectPaginationMetadata()
        {
            var data = new List<SalesSegmentDto> { SalesSegmentBuilders.ValidDto(id: 1) };
            _mockQueryRepo.Setup(r => r.GetAllAsync(2, 5, "Finance")).ReturnsAsync((data, 20));

            var query = new GetAllSalesSegmentQuery { PageNumber = 2, PageSize = 5, SearchTerm = "Finance" };
            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.TotalCount.Should().Be(20);
            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList_AndDoesNotCallCurrencyLookup()
        {
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null))
                .ReturnsAsync((new List<SalesSegmentDto>(), 0));

            var query = new GetAllSalesSegmentQuery { PageNumber = 1, PageSize = 10, SearchTerm = null };
            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Data.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
            _mockCurrencyLookup.Verify(
                c => c.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_ItemsWithCurrencyId_PopulatesCurrencyName()
        {
            var data = new List<SalesSegmentDto>
            {
                SalesSegmentBuilders.ValidDto(id: 1, currencyId: 5)
            };
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null)).ReturnsAsync((data, 1));
            _mockCurrencyLookup
                .Setup(c => c.GetByIdsAsync(It.Is<IEnumerable<int>>(ids => ids.Contains(5)), It.IsAny<CancellationToken>()))
                .ReturnsAsync(SalesSegmentBuilders.ValidCurrencyList(5));

            var query = new GetAllSalesSegmentQuery { PageNumber = 1, PageSize = 10, SearchTerm = null };
            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Data[0].CurrencyName.Should().Be("US Dollar");
        }

        [Fact]
        public async Task Handle_ItemsWithoutCurrencyId_DoesNotCallCurrencyLookup()
        {
            var data = new List<SalesSegmentDto>
            {
                SalesSegmentBuilders.ValidDto(id: 1, currencyId: null)
            };
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null)).ReturnsAsync((data, 1));

            var query = new GetAllSalesSegmentQuery { PageNumber = 1, PageSize = 10, SearchTerm = null };
            await CreateSut().Handle(query, CancellationToken.None);

            _mockCurrencyLookup.Verify(
                c => c.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_ReturnsSuccessMessage()
        {
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null))
                .ReturnsAsync((new List<SalesSegmentDto>(), 0));

            var query = new GetAllSalesSegmentQuery { PageNumber = 1, PageSize = 10, SearchTerm = null };
            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Message.Should().Contain("retrieved");
        }
    }
}
