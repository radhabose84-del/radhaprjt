using Contracts.Common;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.ILedgerBalance;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Application.JournalMaster.LedgerBalance.Queries.GetAllLedgerBalance;
using MediatR;

namespace FinanceManagement.UnitTests.Application.LedgerBalance
{
    public sealed class GetAllLedgerBalanceQueryHandlerTests
    {
        private readonly Mock<ILedgerBalanceQueryRepository> _query = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _ip = new(MockBehavior.Loose);
        private readonly Mock<IFinancialYearLookup> _fy = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mediator = new(MockBehavior.Loose);

        private GetAllLedgerBalanceQueryHandler CreateSut() => new(_query.Object, _ip.Object, _fy.Object, _mediator.Object);

        [Fact]
        public async Task Handle_ScopesToCompany_AndPassesFilters()
        {
            _ip.Setup(s => s.GetCompanyId()).Returns(1);
            _query.Setup(r => r.GetAllAsync(2, 25, 1, 4, 3, 5200101, 8, 9, 7, "sal"))
                .ReturnsAsync((new List<LedgerBalanceDto> { new() { Id = 1, AccountName = "Salaries", Balance = 1000m, FinancialYearId = 3 } }, 1));
            _fy.Setup(f => f.GetAllFinancialYearAsync())
                .ReturnsAsync(new List<FinancialYearLookupDto> { new() { FinancialYearId = 3, FinancialYearName = "2026-27" } });

            var result = await CreateSut().Handle(new GetAllLedgerBalanceQuery
            {
                PageNumber = 2, PageSize = 25, AccountingPeriodId = 4, FinancialYearId = 3,
                GlAccountId = 5200101, AccountTypeId = 8, AccountGroupId = 9, CostCentreId = 7, SearchTerm = "sal"
            }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().ContainSingle().Which.AccountName.Should().Be("Salaries");
            result.Data![0].FinancialYearName.Should().Be("2026-27");
            result.TotalCount.Should().Be(1);
            _query.Verify(r => r.GetAllAsync(2, 25, 1, 4, 3, 5200101, 8, 9, 7, "sal"), Times.Once);
        }

        [Fact]
        public async Task Handle_NoActiveCompany_Throws()
        {
            _ip.Setup(s => s.GetCompanyId()).Returns((int?)null);

            Func<Task> act = async () => await CreateSut().Handle(new GetAllLedgerBalanceQuery(), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>();
        }
    }
}
