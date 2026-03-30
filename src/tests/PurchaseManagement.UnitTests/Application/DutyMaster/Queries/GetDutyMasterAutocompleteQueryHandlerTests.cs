using PurchaseManagement.Application.Common.Interfaces.IPurchase.DutyMaster;
using PurchaseManagement.Application.DutyMaster;
using PurchaseManagement.Application.DutyMaster.Queries.GetDutyMasterAutocomplete;

namespace PurchaseManagement.UnitTests.Application.DutyMaster.Queries
{
    public sealed class GetDutyMasterAutocompleteQueryHandlerTests
    {
        private readonly Mock<IDutyMasterQueryRepository> _mockReadRepo = new(MockBehavior.Strict);

        private GetDutyMasterAutocompleteQueryHandler CreateSut() =>
            new(_mockReadRepo.Object);

        [Fact]
        public async Task Handle_ReturnsItems()
        {
            var items = new List<DutyMasterAutocompleteDto>
            {
                new DutyMasterAutocompleteDto { Id = 1, DutyCode = "DC001", TariffNumber = "1234.56" }
            };
            _mockReadRepo
                .Setup(r => r.GetAutocompleteAsync("DC", It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<DutyMasterAutocompleteDto>)items);

            var result = await CreateSut().Handle(
                new GetDutyMasterAutocompleteQuery("DC"), CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyTerm_ReturnsAll()
        {
            _mockReadRepo
                .Setup(r => r.GetAutocompleteAsync(null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<DutyMasterAutocompleteDto>());

            var result = await CreateSut().Handle(
                new GetDutyMasterAutocompleteQuery(null), CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_CallsGetAutocompleteOnce()
        {
            _mockReadRepo
                .Setup(r => r.GetAutocompleteAsync("DC", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<DutyMasterAutocompleteDto>());

            await CreateSut().Handle(
                new GetDutyMasterAutocompleteQuery("DC"), CancellationToken.None);

            _mockReadRepo.Verify(
                r => r.GetAutocompleteAsync("DC", It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
