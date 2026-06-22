using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IVoucherTypeMaster;
using FinanceManagement.Application.VoucherType.Dto;
using FinanceManagement.Application.VoucherType.Queries.GetVoucherTypeSummary;

namespace FinanceManagement.UnitTests.Application.VoucherType.Queries
{
    public sealed class GetVoucherTypeSummaryQueryHandlerTests
    {
        private readonly Mock<IVoucherTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetVoucherTypeSummaryQueryHandler CreateSut()
        {
            _mockIp.Setup(s => s.GetCompanyId()).Returns(1);
            return new(_mockQueryRepo.Object, _mockIp.Object, _mockMediator.Object);
        }

        [Fact]
        public async Task Handle_ReturnsCounts()
        {
            var summary = new VoucherTypeSummaryDto { TotalCount = 7, ActiveCount = 6, SystemCount = 4, CustomCount = 3 };
            _mockQueryRepo.Setup(r => r.GetSummaryAsync(1)).ReturnsAsync(summary);

            var result = await CreateSut().Handle(new GetVoucherTypeSummaryQuery(), CancellationToken.None);

            result.TotalCount.Should().Be(7);
            result.ActiveCount.Should().Be(6);
            result.SystemCount.Should().Be(4);
            result.CustomCount.Should().Be(3);
        }
    }
}
