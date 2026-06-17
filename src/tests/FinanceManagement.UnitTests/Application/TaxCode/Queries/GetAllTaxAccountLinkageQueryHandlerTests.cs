using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.ITaxCode;
using FinanceManagement.Application.TaxCode.Dto;
using FinanceManagement.Application.TaxCode.Queries.GetAllTaxAccountLinkage;

namespace FinanceManagement.UnitTests.Application.TaxCode.Queries
{
    public sealed class GetAllTaxAccountLinkageQueryHandlerTests
    {
        private readonly Mock<ITaxCodeQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        public GetAllTaxAccountLinkageQueryHandlerTests()
        {
            _mockIp.Setup(x => x.GetCompanyId()).Returns(1);
        }

        private GetAllTaxAccountLinkageQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockIp.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess_WithPagination()
        {
            var data = new List<TaxAccountLinkageDto> { new() { Id = 1, GlAccountId = 412 } };
            _mockQueryRepo.Setup(r => r.GetAllLinkagesAsync(1, 10, null, 1, null)).ReturnsAsync((data, 1));
            _mockMapper.Setup(m => m.Map<List<TaxAccountLinkageDto>>(It.IsAny<List<TaxAccountLinkageDto>>())).Returns<List<TaxAccountLinkageDto>>(x => x);

            var result = await CreateSut().Handle(new GetAllTaxAccountLinkageQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.TotalCount.Should().Be(1);
            result.Data.Should().HaveCount(1);
        }
    }
}
