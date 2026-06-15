using FinanceManagement.Application.AccountTypeMaster.Commands.CreateAccountTypeMaster;
using FinanceManagement.Application.AccountTypeMaster.Commands.DeleteAccountTypeMaster;
using FinanceManagement.Application.AccountTypeMaster.Commands.UpdateAccountTypeMaster;
using FinanceManagement.Application.AccountTypeMaster.Queries.GetAccountTypeMasterAutoComplete;
using FinanceManagement.Application.AccountTypeMaster.Queries.GetAccountTypeMasterById;
using FinanceManagement.Application.AccountTypeMaster.Queries.GetAllAccountTypeMaster;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagement.Presentation.Controllers
{
    [Route("api/finance/[controller]")]
    public class AccountTypeMasterController : ApiControllerBase
    {
        public AccountTypeMasterController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllAccountTypeMasterAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null,
            [FromQuery] int? CompanyId = null)
        {
            var result = await Mediator.Send(new GetAllAccountTypeMasterQuery
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                SearchTerm = SearchTerm,
                CompanyId = CompanyId
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAccountTypeMasterByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetAccountTypeMasterByIdQuery { Id = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetAccountTypeMasterAutoCompleteAsync(
            [FromQuery] string? term = null,
            [FromQuery] int? CompanyId = null)
        {
            var result = await Mediator.Send(new GetAccountTypeMasterAutoCompleteQuery(term ?? string.Empty, CompanyId));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateAccountTypeMaster([FromBody] CreateAccountTypeMasterCommand command)
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
        public async Task<IActionResult> UpdateAccountTypeMaster([FromBody] UpdateAccountTypeMasterCommand command)
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
        public async Task<IActionResult> DeleteAccountTypeMaster(int id)
        {
            var result = await Mediator.Send(new DeleteAccountTypeMasterCommand(id));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Account Type Master deleted successfully." : "Failed to delete Account Type Master."
            });
        }
    }
}
