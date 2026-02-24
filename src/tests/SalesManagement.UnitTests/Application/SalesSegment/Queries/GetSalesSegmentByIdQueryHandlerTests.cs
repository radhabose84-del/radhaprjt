#nullable disable
using Contracts.Interfaces.Lookups.Users;
using SalesManagement.Application.Common.Interfaces.ISalesSegment;
using SalesManagement.Application.SalesSegment.Dto;
using SalesManagement.Application.SalesSegment.Queries.GetSalesSegmentById;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.SalesSegment.Queries
{
    /// <summary>
    /// ⚠️ GetById returns IsSuccess=false (no exception thrown) when entity not found.
    /// Currency lookup is only called when entity has a CurrencyId.
    /// </summary>
    public class GetSalesSegmentByIdQueryHandlerTests
    {
        private readonly Mock<ISalesSegmentQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<ICurrencyLookup> _mockCurrencyLookup = new(MockBehavior.Strict);

        private GetSalesSegmentByIdQueryHandler CreateSut() =>
            new GetSalesSegmentByIdQueryHandler(_mockQueryRepo.Object, _mockCurrencyLookup.Object);

        // ── Tests ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_EntityFound_NoCurrency_ReturnsSuccess()
        {
            var query = new GetSalesSegmentByIdQuery { Id = 1 };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(SalesSegmentBuilders.ValidDto(id: 1, currencyId: null));

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_EntityFound_ReturnsCorrectData()
        {
            var query = new GetSalesSegmentByIdQuery { Id = 1 };
            var dto = SalesSegmentBuilders.ValidDto(id: 1, segmentName: "Finance Segment", currencyId: null);
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Data.Should().NotBeNull();
            result.Data.Id.Should().Be(1);
            result.Data.SegmentName.Should().Be("Finance Segment");
        }

        [Fact]
        public async Task Handle_EntityFound_ReturnsSuccessMessage()
        {
            var query = new GetSalesSegmentByIdQuery { Id = 1 };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(SalesSegmentBuilders.ValidDto(id: 1, currencyId: null));

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Message.Should().Contain("retrieved");
        }

        [Fact]
        public async Task Handle_EntityFound_WithCurrency_PopulatesCurrencyName()
        {
            var query = new GetSalesSegmentByIdQuery { Id = 1 };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(SalesSegmentBuilders.ValidDto(id: 1, currencyId: 5));
            _mockCurrencyLookup
                .Setup(c => c.GetByIdsAsync(It.Is<IEnumerable<int>>(ids => ids.Contains(5)), It.IsAny<CancellationToken>()))
                .ReturnsAsync(SalesSegmentBuilders.ValidCurrencyList(5));

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Data.CurrencyName.Should().Be("US Dollar");
        }

        [Fact]
        public async Task Handle_EntityNotFound_ReturnsFailure()
        {
            // ⚠️ Returns IsSuccess=false — does NOT throw EntityNotFoundException
            var query = new GetSalesSegmentByIdQuery { Id = 99 };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((SalesSegmentDto)null);

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_EntityNotFound_DoesNotCallCurrencyLookup()
        {
            var query = new GetSalesSegmentByIdQuery { Id = 99 };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((SalesSegmentDto)null);

            await CreateSut().Handle(query, CancellationToken.None);

            _mockCurrencyLookup.Verify(
                c => c.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
