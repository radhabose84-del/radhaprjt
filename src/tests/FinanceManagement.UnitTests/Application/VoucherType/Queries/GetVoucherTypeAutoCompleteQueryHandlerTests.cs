using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IVoucherTypeMaster;
using FinanceManagement.Application.VoucherType.Dto;
using FinanceManagement.Application.VoucherType.Queries.GetVoucherTypeAutoComplete;
using FinanceManagement.UnitTests.TestData;

namespace FinanceManagement.UnitTests.Application.VoucherType.Queries
{
    public sealed class GetVoucherTypeAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IVoucherTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetVoucherTypeAutoCompleteQueryHandler CreateSut()
        {
            _mockIp.Setup(s => s.GetCompanyId()).Returns(1);
            return new(_mockQueryRepo.Object, _mockIp.Object, _mockMapper.Object, _mockMediator.Object);
        }

        [Fact]
        public async Task Handle_ReturnsLookupList()
        {
            var lookups = VoucherTypeBuilders.ValidLookupList();
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync("JV", 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookups);
            _mockMapper
                .Setup(m => m.Map<List<VoucherTypeLookupDto>>(It.IsAny<object>()))
                .Returns(lookups);

            var result = await CreateSut().Handle(new GetVoucherTypeAutoCompleteQuery("JV"), CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].VoucherTypeCode.Should().Be("JV");
        }

        [Fact]
        public async Task Handle_NoMatch_ReturnsEmpty()
        {
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync("zzz", 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<VoucherTypeLookupDto>());
            _mockMapper
                .Setup(m => m.Map<List<VoucherTypeLookupDto>>(It.IsAny<object>()))
                .Returns(new List<VoucherTypeLookupDto>());

            var result = await CreateSut().Handle(new GetVoucherTypeAutoCompleteQuery("zzz"), CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
