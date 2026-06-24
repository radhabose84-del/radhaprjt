using FinanceManagement.Application.JournalMaster.LedgerBalance.Queries.GetAllLedgerBalance;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagement.Presentation.Controllers.JournalMaster
{
    [Route("api/finance/[controller]")]
    public class LedgerBalanceController : FinanceManagement.Presentation.Controllers.ApiControllerBase
    {
        public LedgerBalanceController(IMediator mediator) : base(mediator) { }

        // Period ledger balances enriched with GL account / account type / account group. Company from session.
        [HttpGet]
        public async Task<IActionResult> GetAllAsync(
            [FromQuery] int PageNumber = 1,
            [FromQuery] int PageSize = 50,
            [FromQuery] int? AccountingPeriodId = null,
            [FromQuery] int? FinancialYearId = null,
            [FromQuery] int? GlAccountId = null,
            [FromQuery] int? AccountTypeId = null,
            [FromQuery] int? AccountGroupId = null,
            [FromQuery] int? CostCentreId = null,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllLedgerBalanceQuery
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                AccountingPeriodId = AccountingPeriodId,
                FinancialYearId = FinancialYearId,
                GlAccountId = GlAccountId,
                AccountTypeId = AccountTypeId,
                AccountGroupId = AccountGroupId,
                CostCentreId = CostCentreId,
                SearchTerm = SearchTerm
            });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result.Data,
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            });
        }
    }
}
