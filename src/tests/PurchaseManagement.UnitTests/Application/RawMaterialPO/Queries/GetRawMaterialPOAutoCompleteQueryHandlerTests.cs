using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IRawMaterialPO;
using PurchaseManagement.Application.RawMaterialPO.Queries.GetRawMaterialPOAutoComplete;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.RawMaterialPO.Queries
{
    public sealed class GetRawMaterialPOAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IRawMaterialPOQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetRawMaterialPOAutoCompleteQueryHandler CreateSut() => new(_mockQueryRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsMatches()
        {
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(RawMaterialPOBuilders.ValidLookupList());

            var result = await CreateSut().Handle(new GetRawMaterialPOAutoCompleteQuery("RMPO"), CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].PONumber.Should().Be("RMPO-2026-0001");
        }

        [Fact]
        public async Task Handle_NullTerm_PassesEmptyString()
        {
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync(string.Empty, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(RawMaterialPOBuilders.ValidLookupList());

            var result = await CreateSut().Handle(new GetRawMaterialPOAutoCompleteQuery(null!), CancellationToken.None);

            result.Should().HaveCount(1);
            _mockQueryRepo.Verify(r => r.AutocompleteAsync(string.Empty, It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
