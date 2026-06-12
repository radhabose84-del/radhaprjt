using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IMixCodeMaster;
using PurchaseManagement.Application.MixCodeMaster.Dto;
using PurchaseManagement.Application.MixCodeMaster.Queries.GetMixCodeMasterAutoComplete;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.MixCodeMaster.Queries
{
    public sealed class GetMixCodeMasterAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IMixCodeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMixCodeMasterAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsList()
        {
            IReadOnlyList<MixCodeMasterLookupDto> lookups = new List<MixCodeMasterLookupDto> { MixCodeMasterBuilders.ValidLookup() };
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookups);

            var result = await CreateSut().Handle(new GetMixCodeMasterAutoCompleteQuery("MIX"), CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_CallsAutocompleteOnce()
        {
            IReadOnlyList<MixCodeMasterLookupDto> lookups = new List<MixCodeMasterLookupDto>();
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookups);

            await CreateSut().Handle(new GetMixCodeMasterAutoCompleteQuery(null), CancellationToken.None);

            _mockQueryRepo.Verify(
                r => r.AutocompleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
