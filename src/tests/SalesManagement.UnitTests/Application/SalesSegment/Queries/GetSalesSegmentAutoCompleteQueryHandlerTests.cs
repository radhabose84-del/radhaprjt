using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesSegment;
using SalesManagement.Application.SalesSegment.Dto;
using SalesManagement.Application.SalesSegment.Queries.GetSalesSegmentAutoComplete;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.SalesSegment.Queries
{
    /// <summary>
    /// ⚠️ SalesSegment AutoComplete passes Term directly — no null→"" conversion.
    /// </summary>
    public class GetSalesSegmentAutoCompleteQueryHandlerTests
    {
        private readonly Mock<ISalesSegmentQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new();
        private readonly Mock<IMediator> _mockMediator = new();

        private GetSalesSegmentAutoCompleteQueryHandler CreateSut()
        {
            _mockMapper.Setup(m => m.Map<List<SalesSegmentLookupDto>>(It.IsAny<object>()))
                .Returns<object>(o => o is IEnumerable<SalesSegmentLookupDto> e ? e.ToList() : new List<SalesSegmentLookupDto>());
            _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            return new GetSalesSegmentAutoCompleteQueryHandler(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
        }

        // ── Tests ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_WithTerm_ReturnsLookupList()
        {
            var query = new GetSalesSegmentAutoCompleteQuery("Seg");
            var lookupList = SalesSegmentBuilders.ValidLookupList();
            _mockQueryRepo.Setup(r => r.AutocompleteAsync("Seg", It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookupList);

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_WithTerm_ReturnsCorrectLookupData()
        {
            var query = new GetSalesSegmentAutoCompleteQuery("Seg");
            var lookupList = SalesSegmentBuilders.ValidLookupList();
            _mockQueryRepo.Setup(r => r.AutocompleteAsync("Seg", It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookupList);

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result[0].SegmentName.Should().Be("Segment One");
            result[1].SegmentName.Should().Be("Segment Two");
        }

        [Fact]
        public async Task Handle_WithTerm_CallsAutocompleteAsync_Once()
        {
            var query = new GetSalesSegmentAutoCompleteQuery("test");
            _mockQueryRepo.Setup(r => r.AutocompleteAsync("test", It.IsAny<CancellationToken>()))
                .ReturnsAsync(SalesSegmentBuilders.ValidLookupList());

            await CreateSut().Handle(query, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.AutocompleteAsync("test", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_NullTerm_PassesNullToRepository()
        {
            var query = new GetSalesSegmentAutoCompleteQuery(null!);
            _mockQueryRepo.Setup(r => r.AutocompleteAsync(null!, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<SalesSegmentLookupDto>());

            await CreateSut().Handle(query, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.AutocompleteAsync(null!, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_EmptyTerm_ReturnsEmptyList()
        {
            var query = new GetSalesSegmentAutoCompleteQuery(string.Empty);
            _mockQueryRepo.Setup(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<SalesSegmentLookupDto>());

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
