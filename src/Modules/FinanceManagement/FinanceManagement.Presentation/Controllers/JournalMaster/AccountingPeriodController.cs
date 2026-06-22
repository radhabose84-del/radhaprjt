using FinanceManagement.Application.JournalMaster.AccountingPeriod.Commands.CreateAccountingPeriod;
using FinanceManagement.Application.JournalMaster.AccountingPeriod.Commands.DeleteAccountingPeriod;
using FinanceManagement.Application.JournalMaster.AccountingPeriod.Commands.UpdateAccountingPeriod;
using FinanceManagement.Application.JournalMaster.AccountingPeriod.Queries.GetAccountingPeriodAutoComplete;
using FinanceManagement.Application.JournalMaster.AccountingPeriod.Queries.GetAccountingPeriodById;
using FinanceManagement.Application.JournalMaster.AccountingPeriod.Queries.GetAllAccountingPeriod;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagement.Presentation.Controllers.JournalMaster
{
    [Route("api/finance/[controller]")]
    public class AccountingPeriodController : FinanceManagement.Presentation.Controllers.ApiControllerBase
    {
        public AccountingPeriodController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllAccountingPeriodAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null,
            [FromQuery] int? FinancialYearId = null)
        {
            var result = await Mediator.Send(new GetAllAccountingPeriodQuery
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                SearchTerm = SearchTerm,
                FinancialYearId = FinancialYearId
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

        [HttpGet("by-name")]
        public async Task<IActionResult> GetAccountingPeriodAutoCompleteAsync(
            [FromQuery] string? term = null,
            [FromQuery] int? financialYearId = null)
        {
            var result = await Mediator.Send(new GetAccountingPeriodAutoCompleteQuery(term ?? string.Empty, financialYearId));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAccountingPeriodByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetAccountingPeriodByIdQuery { Id = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateAccountingPeriod([FromBody] CreateAccountingPeriodCommand command)
        {
            var result = await Mediator.Send(command);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result.IsSuccess,
                message = result.Message,
                data = result.Data
            });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAccountingPeriod([FromBody] UpdateAccountingPeriodCommand command)
        {
            var result = await Mediator.Send(command);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result.IsSuccess,
                message = result.Message,
                data = result.Data
            });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAccountingPeriod(int id)
        {
            var result = await Mediator.Send(new DeleteAccountingPeriodCommand(id));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Accounting Period deleted successfully." : "Failed to delete Accounting Period."
            });
        }
    }
}
