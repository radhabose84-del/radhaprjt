using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.MiscMaster.Dto;
using SalesManagement.Application.MiscMaster.Queries.GetMiscMasterAutoComplete;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.MiscMaster.Queries
{
    public sealed class GetMiscMasterAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new();
        private readonly Mock<IMediator> _mockMediator = new();

        private GetMiscMasterAutoCompleteQueryHandler CreateSut()
        {
            _mockMapper.Setup(m => m.Map<List<MiscMasterLookupDto>>(It.IsAny<object>()))
                .Returns<object>(o => o is IEnumerable<MiscMasterLookupDto> e ? e.ToList() : new List<MiscMasterLookupDto>());
            _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            return new GetMiscMasterAutoCompleteQueryHandler(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
        }

        // ── Happy Path ────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_MatchingTerm_ReturnsResults()
        {
            var lookupList = MiscMasterBuilders.ValidLookupList();
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync("CODE", null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookupList);

            var result = await CreateSut().Handle(
                new GetMiscMasterAutoCompleteQuery("CODE", null),
                CancellationToken.None);

            result.Should().NotBeEmpty();
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_EmptyTerm_ReturnsAllResults()
        {
            var lookupList = MiscMasterBuilders.ValidLookupList();
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync(string.Empty, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookupList);

            var result = await CreateSut().Handle(
                new GetMiscMasterAutoCompleteQuery(string.Empty, null),
                CancellationToken.None);

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_NoMatch_ReturnsEmptyList()
        {
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync("NOMATCH", null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<MiscMasterLookupDto>());

            var result = await CreateSut().Handle(
                new GetMiscMasterAutoCompleteQuery("NOMATCH", null),
                CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_WithMiscTypeId_PassesFilterToRepository()
        {
            var lookupList = MiscMasterBuilders.ValidLookupList();
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync("CODE", 2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookupList);

            var result = await CreateSut().Handle(
                new GetMiscMasterAutoCompleteQuery("CODE", 2),
                CancellationToken.None);

            result.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Handle_ReturnsReadOnlyList()
        {
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(MiscMasterBuilders.ValidLookupList());

            var result = await CreateSut().Handle(
                new GetMiscMasterAutoCompleteQuery("test", null),
                CancellationToken.None);

            result.Should().BeAssignableTo<IReadOnlyList<MiscMasterLookupDto>>();
        }
    }
}
