using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IPeriodStatusOverride;
using FinanceManagement.Application.PeriodStatusOverride.Dto;
using FinanceManagement.Application.PeriodStatusOverride.Queries.GetFinancialPeriodStatus;

namespace FinanceManagement.UnitTests.Application.PeriodStatusOverride.Queries
{
    public sealed class GetFinancialPeriodStatusQueryHandlerTests
    {
        private readonly Mock<IPeriodStatusOverrideQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetFinancialPeriodStatusQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockIp.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingPeriod_ReturnsStatusDto()
        {
            _mockIp.Setup(x => x.GetCompanyId()).Returns(1);
            _mockQueryRepo.Setup(r => r.GetPeriodStatusAsync(1, 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FinancialPeriodStatusDto { PeriodId = 1, StatusCode = "OPEN" });

            var result = await CreateSut().Handle(new GetFinancialPeriodStatusQuery(1), CancellationToken.None);

            result.Should().NotBeNull();
            result!.StatusCode.Should().Be("OPEN");
        }

        [Fact]
        public async Task Handle_PeriodNotFound_ReturnsNull()
        {
            _mockIp.Setup(x => x.GetCompanyId()).Returns(1);
            _mockQueryRepo.Setup(r => r.GetPeriodStatusAsync(99, 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync((FinancialPeriodStatusDto?)null);

            var result = await CreateSut().Handle(new GetFinancialPeriodStatusQuery(99), CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_NoCompany_Throws()
        {
            _mockIp.Setup(x => x.GetCompanyId()).Returns((int?)null);

            Func<Task> act = () => CreateSut().Handle(new GetFinancialPeriodStatusQuery(1), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>();
        }
    }
}
