using AutoMapper;
using MediatR;
using QCManagement.Application.Common.Interfaces.IMiscMaster;
using QCManagement.Application.MiscMaster.Dto;
using QCManagement.Application.MiscMaster.Queries.GetMiscMasterAutoComplete;
using QCManagement.UnitTests.TestData;

namespace QCManagement.UnitTests.Application.MiscMaster.Queries
{
    public class GetMiscMasterAutoCompleteQueryHandlerTests
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

        [Fact]
        public async Task Handle_WithMatchingTerm_ReturnsLookupList()
        {
            var lookupList = MiscMasterBuilders.ValidLookupList();
            _mockQueryRepo.Setup(r => r.AutocompleteAsync("PHY", "QP_GROUP", It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookupList);

            var result = await CreateSut().Handle(
                new GetMiscMasterAutoCompleteQuery("PHY", "QP_GROUP"), CancellationToken.None);

            result.Should().NotBeNull();
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_ReturnsCorrectLookupData()
        {
            var lookupList = MiscMasterBuilders.ValidLookupList();
            _mockQueryRepo.Setup(r => r.AutocompleteAsync("PHY", "QP_GROUP", It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookupList);

            var result = await CreateSut().Handle(
                new GetMiscMasterAutoCompleteQuery("PHY", "QP_GROUP"), CancellationToken.None);

            result[0].Code.Should().Be("PHY");
            result[1].Code.Should().Be("CHM");
        }

        [Fact]
        public async Task Handle_EmptyTerm_ReturnsEmptyList()
        {
            _mockQueryRepo.Setup(r => r.AutocompleteAsync(string.Empty, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<MiscMasterLookupDto>());

            var result = await CreateSut().Handle(
                new GetMiscMasterAutoCompleteQuery(string.Empty, null), CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_CallsAutocompleteAsync_Once_WithCorrectArguments()
        {
            _mockQueryRepo.Setup(r => r.AutocompleteAsync("PHY", "QP_GROUP", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<MiscMasterLookupDto>());

            await CreateSut().Handle(
                new GetMiscMasterAutoCompleteQuery("PHY", "QP_GROUP"), CancellationToken.None);

            _mockQueryRepo.Verify(
                r => r.AutocompleteAsync("PHY", "QP_GROUP", It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_NoMatch_ReturnsEmptyList()
        {
            _mockQueryRepo.Setup(r => r.AutocompleteAsync("ZZZZ", null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<MiscMasterLookupDto>());

            var result = await CreateSut().Handle(
                new GetMiscMasterAutoCompleteQuery("ZZZZ", null), CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
