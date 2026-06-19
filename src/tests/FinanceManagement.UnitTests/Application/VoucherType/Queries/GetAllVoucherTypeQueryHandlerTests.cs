using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IVoucherTypeMaster;
using FinanceManagement.Application.VoucherType.Dto;
using FinanceManagement.Application.VoucherType.Queries.GetAllVoucherType;
using FinanceManagement.UnitTests.TestData;

namespace FinanceManagement.UnitTests.Application.VoucherType.Queries
{
    public sealed class GetAllVoucherTypeQueryHandlerTests
    {
        private readonly Mock<IVoucherTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAllVoucherTypeQueryHandler CreateSut()
        {
            _mockIp.Setup(s => s.GetCompanyId()).Returns(1);
            return new(_mockQueryRepo.Object, _mockIp.Object, _mockMapper.Object, _mockMediator.Object);
        }

        private void SetupMapper(List<VoucherTypeMasterDto> dtos) =>
            _mockMapper
                .Setup(m => m.Map<List<VoucherTypeMasterDto>>(It.IsAny<object>()))
                .Returns(dtos);

        [Fact]
        public async Task Handle_ReturnsSuccessWithData()
        {
            var dtos = new List<VoucherTypeMasterDto> { VoucherTypeBuilders.ValidDto() };
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null, 1, null)).ReturnsAsync((dtos, 1));
            SetupMapper(dtos);

            var result = await CreateSut().Handle(new GetAllVoucherTypeQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var dtos = new List<VoucherTypeMasterDto> { VoucherTypeBuilders.ValidDto() };
            _mockQueryRepo.Setup(r => r.GetAllAsync(2, 5, "JV", 1, 3)).ReturnsAsync((dtos, 11));
            SetupMapper(dtos);

            var result = await CreateSut().Handle(
                new GetAllVoucherTypeQuery { PageNumber = 2, PageSize = 5, SearchTerm = "JV", FinancialYearId = 3 },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null, 1, null)).ReturnsAsync((new List<VoucherTypeMasterDto>(), 0));
            SetupMapper(new List<VoucherTypeMasterDto>());

            var result = await CreateSut().Handle(new GetAllVoucherTypeQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }
    }
}
