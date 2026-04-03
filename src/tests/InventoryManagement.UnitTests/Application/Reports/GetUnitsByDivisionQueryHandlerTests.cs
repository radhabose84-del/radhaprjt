using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using InventoryManagement.Application.Reports.GetUnitsByDivision;

namespace InventoryManagement.UnitTests.Application.Reports
{
    public sealed class GetUnitsByDivisionQueryHandlerTests
    {
        private readonly Mock<IDivisionUnitLookup> _mockLookup = new(MockBehavior.Strict);

        private GetUnitsByDivisionQueryHandler CreateSut() => new(_mockLookup.Object);

        [Fact]
        public async Task Handle_ReturnsLookupResults()
        {
            var expected = new List<DivisionUnitLookupDto>
            {
                new DivisionUnitLookupDto { UnitId = 1, UnitName = "Unit A" }
            };
            _mockLookup
                .Setup(l => l.GetUnitsByDivisionAsync(1, 2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await CreateSut().Handle(
                new GetUnitsByDivisionQuery { CompanyId = 1, DivisionId = 2 },
                CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].UnitName.Should().Be("Unit A");
        }

        [Fact]
        public async Task Handle_EmptyResults_ReturnsEmptyList()
        {
            _mockLookup
                .Setup(l => l.GetUnitsByDivisionAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<DivisionUnitLookupDto>());

            var result = await CreateSut().Handle(
                new GetUnitsByDivisionQuery { CompanyId = 1, DivisionId = 1 },
                CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_CallsLookupOnce()
        {
            _mockLookup
                .Setup(l => l.GetUnitsByDivisionAsync(5, 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<DivisionUnitLookupDto>());

            await CreateSut().Handle(
                new GetUnitsByDivisionQuery { CompanyId = 5, DivisionId = 10 },
                CancellationToken.None);

            _mockLookup.Verify(
                l => l.GetUnitsByDivisionAsync(5, 10, It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
