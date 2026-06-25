using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IPeriodStatusOverride;
using FinanceManagement.Application.PeriodStatusOverride.Dto;
using FinanceManagement.Application.PeriodStatusOverride.Queries.GetPeriodStatusHistory;
using FinanceManagement.UnitTests.TestData;

namespace FinanceManagement.UnitTests.Application.PeriodStatusOverride.Queries
{
    public sealed class GetPeriodStatusHistoryQueryHandlerTests
    {
        private readonly Mock<IPeriodStatusOverrideQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetPeriodStatusHistoryQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockIp.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsAllHistoryForPeriod()
        {
            _mockIp.Setup(x => x.GetCompanyId()).Returns(1);
            IReadOnlyList<PeriodStatusOverrideDto> rows = new List<PeriodStatusOverrideDto>
            {
                PeriodStatusOverrideBuilders.PendingOverrideDto(id: 1),
                PeriodStatusOverrideBuilders.PendingOverrideDto(id: 2, statusCode: "REJECTED")
            };
            _mockQueryRepo.Setup(r => r.GetHistoryForPeriodAsync(1, 1, It.IsAny<CancellationToken>())).ReturnsAsync(rows);

            var result = await CreateSut().Handle(new GetPeriodStatusHistoryQuery(1), CancellationToken.None);

            result.Should().HaveCount(2);
        }
    }
}
