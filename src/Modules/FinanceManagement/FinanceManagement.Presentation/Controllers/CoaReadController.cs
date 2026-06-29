using FinanceManagement.Application.CoaRead.Queries.GetAccountByCode;
using FinanceManagement.Application.CoaRead.Queries.SearchAccountsForRead;
using FinanceManagement.Application.CoaRead.Queries.ValidateForPosting;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagement.Presentation.Controllers
{
    // US-GL02-16 — COA Read API for downstream modules (AP/AR/FA). Read-only; company from JWT;
    // authenticated by global middleware and logged per call. No write endpoints.
    [Route("api/finance/coa")]
    public class CoaReadController : ApiControllerBase
    {
        public CoaReadController(IMediator mediator) : base(mediator) { }

        // AC1 — get account by code (< 100ms).
        [HttpGet("accounts/by-code/{accountCode}")]
        public async Task<IActionResult> GetByCodeAsync(string accountCode)
        {
            var result = await Mediator.Send(new GetAccountByCodeQuery(accountCode));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result.IsSuccess,
                message = result.Message,
                data = result.Data
            });
        }

        // AC5 — search by type/group; each row carries its active status.
        [HttpGet("accounts")]
        public async Task<IActionResult> SearchAsync(
            [FromQuery] int? AccountTypeId = null,
            [FromQuery] int? AccountGroupId = null,
            [FromQuery] bool ActiveOnly = false)
        {
            var result = await Mediator.Send(new SearchAccountsForReadQuery
            {
                AccountTypeId = AccountTypeId,
                AccountGroupId = AccountGroupId,
                ActiveOnly = ActiveOnly
            });
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result.Data, TotalCount = result.TotalCount });
        }

        // AC2 — validate an account for posting (active + currency match + cost-centre rule).
        [HttpGet("accounts/validate-for-posting")]
        public async Task<IActionResult> ValidateForPostingAsync(
            [FromQuery] string accountCode,
            [FromQuery] int? currencyId = null,
            [FromQuery] int? costCentreId = null)
        {
            var result = await Mediator.Send(new ValidateForPostingQuery
            {
                AccountCode = accountCode,
                CurrencyId = currencyId,
                CostCentreId = costCentreId
            });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result.IsSuccess,
                message = result.Message,
                data = result.Data
            });
        }
    }
}
