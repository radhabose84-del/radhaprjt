using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IAccountingPeriod;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Application.JournalMaster.AccountingPeriod.Queries.GetAccountingPeriodAutoComplete;
using FinanceManagement.UnitTests.TestData;

namespace FinanceManagement.UnitTests.Application.AccountingPeriod.Queries
{
    public sealed class GetAccountingPeriodAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IAccountingPeriodQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAccountingPeriodAutoCompleteQueryHandler CreateSut()
        {
            _mockIp.Setup(s => s.GetCompanyId()).Returns(1);
            return new(_mockQueryRepo.Object, _mockIp.Object, _mockMapper.Object, _mockMediator.Object);
        }

        [Fact]
        public async Task Handle_ReturnsLookupList()
        {
            var list = AccountingPeriodBuilders.ValidLookupList();
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync("Jun", 1, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(list);
            _mockMapper.Setup(m => m.Map<List<AccountingPeriodLookupDto>>(It.IsAny<object>())).Returns(list);

            var result = await CreateSut().Handle(new GetAccountingPeriodAutoCompleteQuery("Jun"), CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_PassesFinancialYearFilter()
        {
            var list = AccountingPeriodBuilders.ValidLookupList();
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync("Jun", 1, 3, It.IsAny<CancellationToken>()))
                .ReturnsAsync(list);
            _mockMapper.Setup(m => m.Map<List<AccountingPeriodLookupDto>>(It.IsAny<object>())).Returns(list);

            var result = await CreateSut().Handle(new GetAccountingPeriodAutoCompleteQuery("Jun", 3), CancellationToken.None);

            result.Should().HaveCount(1);
            _mockQueryRepo.Verify(r => r.AutocompleteAsync("Jun", 1, 3, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
