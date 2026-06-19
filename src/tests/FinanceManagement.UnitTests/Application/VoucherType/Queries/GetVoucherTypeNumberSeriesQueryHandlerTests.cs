using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IVoucherTypeMaster;
using FinanceManagement.Application.VoucherType.Dto;
using FinanceManagement.Application.VoucherType.Queries.GetVoucherTypeNumberSeries;
using FinanceManagement.UnitTests.TestData;

namespace FinanceManagement.UnitTests.Application.VoucherType.Queries
{
    public sealed class GetVoucherTypeNumberSeriesQueryHandlerTests
    {
        private readonly Mock<IVoucherTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetVoucherTypeNumberSeriesQueryHandler CreateSut()
        {
            _mockIp.Setup(s => s.GetCompanyId()).Returns(1);
            return new(_mockQueryRepo.Object, _mockIp.Object, _mockMediator.Object);
        }

        [Fact]
        public async Task Handle_ReturnsSeriesForFiscalYear()
        {
            var rows = VoucherTypeBuilders.ValidNumberSeriesList();
            _mockQueryRepo.Setup(r => r.GetNumberSeriesAsync(3, 1)).ReturnsAsync(rows);

            var result = await CreateSut().Handle(new GetVoucherTypeNumberSeriesQuery { FinancialYearId = 3 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
            result.Data![0].NextNumber.Should().Be("JV/2026-27/0428");
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            _mockQueryRepo.Setup(r => r.GetNumberSeriesAsync(99, 1)).ReturnsAsync(new List<VoucherTypeNumberSeriesDto>());

            var result = await CreateSut().Handle(new GetVoucherTypeNumberSeriesQuery { FinancialYearId = 99 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }
    }
}
