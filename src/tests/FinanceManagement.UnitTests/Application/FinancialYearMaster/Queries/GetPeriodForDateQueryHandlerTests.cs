using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IFinancialYearMaster;
using FinanceManagement.Application.FinancialYearMaster.Dto;
using FinanceManagement.Application.FinancialYearMaster.Queries.GetPeriodForDate;
using FinanceManagement.UnitTests.TestData;

namespace FinanceManagement.UnitTests.Application.FinancialYearMaster.Queries
{
    public sealed class GetPeriodForDateQueryHandlerTests
    {
        private readonly Mock<IFinancialYearMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _mockIp       = new(MockBehavior.Loose);
        private readonly Mock<IMapper>           _mockMapper   = new(MockBehavior.Loose);
        private readonly Mock<IMediator>         _mockMediator = new(MockBehavior.Loose);

        private GetPeriodForDateQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockIp.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_DateInPeriod_ReturnsPeriodDto()
        {
            _mockIp.Setup(x => x.GetCompanyId()).Returns(1);
            var dto = FinancialYearMasterBuilders.ValidPeriodDto();
            _mockQueryRepo
                .Setup(r => r.GetPeriodForDateAsync(1, new DateOnly(2024, 4, 15), It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);
            _mockMapper.Setup(m => m.Map<FinancialPeriodMasterDto>(It.IsAny<FinancialPeriodMasterDto>())).Returns(dto);

            var result = await CreateSut().Handle(
                new GetPeriodForDateQuery(new DateOnly(2024, 4, 15)),
                CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_NoPeriodMatches_ReturnsNull()
        {
            _mockIp.Setup(x => x.GetCompanyId()).Returns(1);
            _mockQueryRepo
                .Setup(r => r.GetPeriodForDateAsync(1, It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((FinancialPeriodMasterDto?)null);

            var result = await CreateSut().Handle(
                new GetPeriodForDateQuery(new DateOnly(2030, 1, 1)),
                CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_NoCompanyInSession_Throws()
        {
            _mockIp.Setup(x => x.GetCompanyId()).Returns((int?)null);

            Func<Task> act = () => CreateSut().Handle(
                new GetPeriodForDateQuery(new DateOnly(2024, 4, 15)),
                CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>();
        }
    }
}
