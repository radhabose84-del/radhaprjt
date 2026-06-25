using FinanceManagement.Application.Common.Interfaces.IFinancialYearMaster;
using FinanceManagement.Application.FinancialYearMaster.Dto;
using FinanceManagement.Application.FinancialYearMaster.Queries.GetFinancialPeriodsForCompany;
using FinanceManagement.UnitTests.TestData;

namespace FinanceManagement.UnitTests.Application.FinancialYearMaster.Queries
{
    public sealed class GetFinancialPeriodsForCompanyQueryHandlerTests
    {
        private readonly Mock<IFinancialYearMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper>   _mockMapper   = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetFinancialPeriodsForCompanyQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidCompanyId_ReturnsAllPeriods()
        {
            var periods = Enumerable.Range(1, 13)
                .Select(i => FinancialYearMasterBuilders.ValidPeriodDto(id: i, periodNumber: (byte)i, isAdjustment: i == 13))
                .ToList();
            _mockQueryRepo.Setup(r => r.GetPeriodsForCompanyAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(periods);
            _mockMapper
                .Setup(m => m.Map<List<FinancialPeriodMasterDto>>(It.IsAny<IReadOnlyList<FinancialPeriodMasterDto>>()))
                .Returns(periods);

            var result = await CreateSut().Handle(new GetFinancialPeriodsForCompanyQuery(5), CancellationToken.None);

            result.Should().HaveCount(13);
            result.Count(p => p.IsAdjustmentPeriod).Should().Be(1);
        }
    }
}
