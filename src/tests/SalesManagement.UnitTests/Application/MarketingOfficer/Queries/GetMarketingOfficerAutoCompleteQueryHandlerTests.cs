using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IMarketingOfficer;
using SalesManagement.Application.MarketingOfficer.Dto;
using SalesManagement.Application.MarketingOfficer.Queries.GetMarketingOfficerAutoComplete;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.MarketingOfficer.Queries
{
    public class GetMarketingOfficerAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IMarketingOfficerQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new();
        private readonly Mock<IMediator> _mockMediator = new();

        private GetMarketingOfficerAutoCompleteQueryHandler CreateSut()
        {
            _mockMapper.Setup(m => m.Map<List<MarketingOfficerLookupDto>>(It.IsAny<object>()))
                .Returns<object>(o => o is IEnumerable<MarketingOfficerLookupDto> e ? e.ToList() : new List<MarketingOfficerLookupDto>());
            _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            return new GetMarketingOfficerAutoCompleteQueryHandler(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
        }

        // ── Tests ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_WithTerm_ReturnsLookupList()
        {
            var query = new GetMarketingOfficerAutoCompleteQuery("EMP");
            var lookupList = MarketingOfficerBuilders.ValidLookupList();

            _mockQueryRepo.Setup(r => r.AutocompleteAsync("EMP", It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookupList);

            var sut = CreateSut();

            var result = await sut.Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_WithTerm_ReturnsCorrectLookupData()
        {
            var query = new GetMarketingOfficerAutoCompleteQuery("EMP");
            var lookupList = MarketingOfficerBuilders.ValidLookupList();

            _mockQueryRepo.Setup(r => r.AutocompleteAsync("EMP", It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookupList);

            var sut = CreateSut();

            var result = await sut.Handle(query, CancellationToken.None);

            result[0].EmployeeNo.Should().Be("EMP001");
            result[0].EmployeeName.Should().Be("Test Officer");
        }

        [Fact]
        public async Task Handle_WithTerm_CallsAutocompleteAsync_Once()
        {
            var query = new GetMarketingOfficerAutoCompleteQuery("test");
            var lookupList = MarketingOfficerBuilders.ValidLookupList();

            _mockQueryRepo.Setup(r => r.AutocompleteAsync("test", It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookupList);

            var sut = CreateSut();

            await sut.Handle(query, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.AutocompleteAsync("test", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_NullTerm_PassesEmptyStringToRepository()
        {
            var query = new GetMarketingOfficerAutoCompleteQuery(null!);
            var lookupList = new List<MarketingOfficerLookupDto>();

            _mockQueryRepo.Setup(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookupList);

            var sut = CreateSut();

            await sut.Handle(query, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_EmptyTerm_ReturnsEmptyList()
        {
            var query = new GetMarketingOfficerAutoCompleteQuery(string.Empty);
            var emptyList = new List<MarketingOfficerLookupDto>();

            _mockQueryRepo.Setup(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()))
                .ReturnsAsync(emptyList);

            var sut = CreateSut();

            var result = await sut.Handle(query, CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
