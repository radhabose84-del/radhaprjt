using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IFinancialYearMaster;
using FinanceManagement.Application.FinancialYearMaster.Dto;
using FinanceManagement.Application.FinancialYearMaster.Queries.GetFinancialYearMasterAutoComplete;
using FinanceManagement.UnitTests.TestData;

namespace FinanceManagement.UnitTests.Application.FinancialYearMaster.Queries
{
    public sealed class GetFinancialYearMasterAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IFinancialYearMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _mockIp       = new(MockBehavior.Loose);
        private readonly Mock<IMapper>           _mockMapper   = new(MockBehavior.Loose);
        private readonly Mock<IMediator>         _mockMediator = new(MockBehavior.Loose);

        private GetFinancialYearMasterAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockIp.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidTerm_ReturnsLookupList()
        {
            _mockIp.Setup(x => x.GetCompanyId()).Returns(1);

            IReadOnlyList<FinancialYearMasterLookupDto> source = new List<FinancialYearMasterLookupDto>
            {
                FinancialYearMasterBuilders.ValidLookupDto()
            };
            _mockQueryRepo.Setup(r => r.AutocompleteAsync("test", 1, It.IsAny<CancellationToken>())).ReturnsAsync(source);
            _mockMapper
                .Setup(m => m.Map<List<FinancialYearMasterLookupDto>>(It.IsAny<IReadOnlyList<FinancialYearMasterLookupDto>>()))
                .Returns(source.ToList());

            var result = await CreateSut().Handle(new GetFinancialYearMasterAutoCompleteQuery("test"), CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_NoCompany_Throws()
        {
            _mockIp.Setup(x => x.GetCompanyId()).Returns((int?)null);

            Func<Task> act = () => CreateSut().Handle(
                new GetFinancialYearMasterAutoCompleteQuery("anything"), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>();
        }
    }
}
