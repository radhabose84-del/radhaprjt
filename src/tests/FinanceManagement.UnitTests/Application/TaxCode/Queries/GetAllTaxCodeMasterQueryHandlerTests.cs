using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.ITaxCode;
using FinanceManagement.Application.TaxCode.Dto;
using FinanceManagement.Application.TaxCode.Queries.GetAllTaxCodeMaster;

namespace FinanceManagement.UnitTests.Application.TaxCode.Queries
{
    public sealed class GetAllTaxCodeMasterQueryHandlerTests
    {
        private readonly Mock<ITaxCodeQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        public GetAllTaxCodeMasterQueryHandlerTests()
        {
            _mockIp.Setup(x => x.GetCompanyId()).Returns(1);
        }

        private GetAllTaxCodeMasterQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockIp.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess_WithPagination()
        {
            var data = new List<TaxCodeMasterDto> { new() { Id = 1, TaxCode = "GST-OUT-5" } };
            _mockQueryRepo.Setup(r => r.GetAllTaxCodesAsync(1, 10, null, 1, null)).ReturnsAsync((data, 1));
            _mockMapper.Setup(m => m.Map<List<TaxCodeMasterDto>>(It.IsAny<List<TaxCodeMasterDto>>())).Returns<List<TaxCodeMasterDto>>(x => x);

            var result = await CreateSut().Handle(new GetAllTaxCodeMasterQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.TotalCount.Should().Be(1);
            result.PageNumber.Should().Be(1);
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            _mockQueryRepo.Setup(r => r.GetAllTaxCodesAsync(1, 10, null, 1, "TDS")).ReturnsAsync((new List<TaxCodeMasterDto>(), 0));
            _mockMapper.Setup(m => m.Map<List<TaxCodeMasterDto>>(It.IsAny<List<TaxCodeMasterDto>>())).Returns<List<TaxCodeMasterDto>>(x => x);

            var result = await CreateSut().Handle(new GetAllTaxCodeMasterQuery { PageNumber = 1, PageSize = 10, TaxType = "TDS" }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.TotalCount.Should().Be(0);
        }
    }
}
