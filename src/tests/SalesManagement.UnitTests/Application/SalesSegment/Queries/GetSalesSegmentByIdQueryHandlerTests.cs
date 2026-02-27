using AutoMapper;
using Contracts.Interfaces.Lookups.Users;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesSegment;
using SalesManagement.Application.SalesSegment.Dto;
using SalesManagement.Application.SalesSegment.Queries.GetSalesSegmentById;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.SalesSegment.Queries
{
    /// <summary>
    /// GetById returns null when entity not found.
    /// Currency lookup is only called when entity has a CurrencyId.
    /// </summary>
    public class GetSalesSegmentByIdQueryHandlerTests
    {
        private readonly Mock<ISalesSegmentQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<ICurrencyLookup> _mockCurrencyLookup = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new();
        private readonly Mock<IMediator> _mockMediator = new();

        private GetSalesSegmentByIdQueryHandler CreateSut()
        {
            _mockMapper.Setup(m => m.Map<SalesSegmentDto>(It.IsAny<object>()))
                .Returns<object>(o => (o as SalesSegmentDto)!);
            _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            return new GetSalesSegmentByIdQueryHandler(_mockQueryRepo.Object, _mockCurrencyLookup.Object, _mockMapper.Object, _mockMediator.Object);
        }

        // â”€â”€ Tests â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        [Fact]
        public async Task Handle_EntityFound_NoCurrency_ReturnsNotNull()
        {
            var query = new GetSalesSegmentByIdQuery { Id = 1 };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(SalesSegmentBuilders.ValidDto(id: 1, currencyId: null));

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_EntityFound_ReturnsCorrectData()
        {
            var query = new GetSalesSegmentByIdQuery { Id = 1 };
            var dto = SalesSegmentBuilders.ValidDto(id: 1, segmentName: "Finance Segment", currencyId: null);
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result!.SegmentName.Should().Be("Finance Segment");
        }

        [Fact]
        public async Task Handle_EntityFound_PublishesAuditEvent()
        {
            var query = new GetSalesSegmentByIdQuery { Id = 1 };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(SalesSegmentBuilders.ValidDto(id: 1, currencyId: null));

            await CreateSut().Handle(query, CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Once);
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

            result!.CurrencyName.Should().Be("US Dollar");
        }

        [Fact]
        public async Task Handle_EntityNotFound_ReturnsNull()
        {
            var query = new GetSalesSegmentByIdQuery { Id = 99 };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((SalesSegmentDto?)null);

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_EntityNotFound_DoesNotCallCurrencyLookup()
        {
            var query = new GetSalesSegmentByIdQuery { Id = 99 };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((SalesSegmentDto?)null);

            await CreateSut().Handle(query, CancellationToken.None);

            _mockCurrencyLookup.Verify(
                c => c.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}