using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IPeriodStatusOverride;
using FinanceManagement.Application.PeriodStatusOverride.Dto;
using FinanceManagement.Application.PeriodStatusOverride.Queries.GetPendingPeriodReversals;
using FinanceManagement.UnitTests.TestData;

namespace FinanceManagement.UnitTests.Application.PeriodStatusOverride.Queries
{
    public sealed class GetPendingPeriodReversalsQueryHandlerTests
    {
        private readonly Mock<IPeriodStatusOverrideQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetPendingPeriodReversalsQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockIp.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsPendingForSessionCompany()
        {
            _mockIp.Setup(x => x.GetCompanyId()).Returns(7);
            IReadOnlyList<PeriodStatusOverrideDto> rows = new List<PeriodStatusOverrideDto>
            {
                PeriodStatusOverrideBuilders.PendingOverrideDto(id: 1)
            };
            _mockQueryRepo.Setup(r => r.GetPendingForCompanyAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(rows);

            var result = await CreateSut().Handle(new GetPendingPeriodReversalsQuery(), CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_NoCompany_Throws()
        {
            _mockIp.Setup(x => x.GetCompanyId()).Returns((int?)null);

            Func<Task> act = () => CreateSut().Handle(new GetPendingPeriodReversalsQuery(), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>();
        }
    }
}
