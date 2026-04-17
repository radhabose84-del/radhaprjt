using MediatR;
using PartyManagement.Application.Common.Interfaces.IPartyMaster;
using PartyManagement.Application.PartyMaster.Queries.GetPartMasterAutoComplete;
using Xunit;

namespace PartyManagement.UnitTests.Application.PartyMaster.Queries
{
    public sealed class GetPartyMasterAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IPartyMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetPartyMasterAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsMatchingResults()
        {
            var resultList = new List<GetPartyMasterAutoCompleteDto>
            {
                new GetPartyMasterAutoCompleteDto { Id = 1, PartyName = "Test Party" }
            };

            _mockQueryRepo
                .Setup(r => r.GetPartyMasterAutoComplete(It.IsAny<List<int>>(), It.IsAny<string>(), It.IsAny<int?>()))
                .ReturnsAsync(resultList);

            var result = await CreateSut().Handle(
                new GetPartyMasterAutoCompleteQuery { SearchPattern = "Test" },
                CancellationToken.None);

            result.Should().NotBeNull();
            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyTerm_ReturnsEmptyList()
        {
            var emptyList = new List<GetPartyMasterAutoCompleteDto>();

            _mockQueryRepo
                .Setup(r => r.GetPartyMasterAutoComplete(It.IsAny<List<int>>(), It.IsAny<string>(), It.IsAny<int?>()))
                .ReturnsAsync(emptyList);

            var result = await CreateSut().Handle(
                new GetPartyMasterAutoCompleteQuery { SearchPattern = null },
                CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_CallsQueryRepoOnce()
        {
            _mockQueryRepo
                .Setup(r => r.GetPartyMasterAutoComplete(It.IsAny<List<int>>(), It.IsAny<string>(), It.IsAny<int?>()))
                .ReturnsAsync(new List<GetPartyMasterAutoCompleteDto>());

            await CreateSut().Handle(
                new GetPartyMasterAutoCompleteQuery { SearchPattern = "PAR" },
                CancellationToken.None);

            _mockQueryRepo.Verify(
                r => r.GetPartyMasterAutoComplete(It.IsAny<List<int>>(), It.IsAny<string>(), It.IsAny<int?>()),
                Times.Once);
        }
    }
}
