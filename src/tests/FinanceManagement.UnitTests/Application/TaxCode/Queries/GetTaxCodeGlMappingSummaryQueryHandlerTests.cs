using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.ITaxCode;
using FinanceManagement.Application.TaxCode.Dto;
using FinanceManagement.Application.TaxCode.Queries.GetTaxCodeGlMappingSummary;

namespace FinanceManagement.UnitTests.Application.TaxCode.Queries
{
    public sealed class GetTaxCodeGlMappingSummaryQueryHandlerTests
    {
        private readonly Mock<ITaxCodeQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        public GetTaxCodeGlMappingSummaryQueryHandlerTests()
        {
            _mockIp.Setup(x => x.GetCompanyId()).Returns(1);
        }

        private GetTaxCodeGlMappingSummaryQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockIp.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSummary_WithGlAccountCounts()
        {
            var data = new List<TaxCodeGlMappingSummaryDto>
            {
                new() { TaxCodeId = 1, TaxCode = "GST-OUT-5", TaxType = "GST_OUT", CurrentRatePercent = 5m, GlAccountCount = 1, IsActive = true },
                new() { TaxCodeId = 2, TaxCode = "TDS-194C-1", TaxType = "TDS", CurrentRatePercent = 1m, GlAccountCount = 0, IsActive = false }
            };
            _mockQueryRepo.Setup(r => r.GetTaxCodeGlMappingSummaryAsync(1, 10, null, 1, null)).ReturnsAsync((data, 2));

            var result = await CreateSut().Handle(new GetTaxCodeGlMappingSummaryQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.TotalCount.Should().Be(2);
            result.Data.Should().HaveCount(2);
            result.Data![0].GlAccountCount.Should().Be(1);
            result.Data[1].GlAccountCount.Should().Be(0);   // "No GL mapping"
        }
    }
}
