using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.TransactionTypeMaster.Commands.CreateTransactionTypeMaster;
using SalesManagement.Application.TransactionTypeMaster.Commands.UpdateTransactionTypeMaster;
using SalesManagement.Application.TransactionTypeMaster.Commands.DeleteTransactionTypeMaster;
using SalesManagement.Application.TransactionTypeMaster.Queries.GetAllTransactionTypeMaster;
using SalesManagement.Application.TransactionTypeMaster.Queries.GetTransactionTypeMasterById;
using SalesManagement.Application.TransactionTypeMaster.Queries.GetTransactionTypeMasterAutoComplete;

namespace SalesManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class TransactionTypeMasterController : ApiControllerBase
    {
        public TransactionTypeMasterController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllTransactionTypeMasterAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllTransactionTypeMasterQuery
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTransactionTypeMasterByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetTransactionTypeMasterByIdQuery { Id = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetTransactionTypeMasterAutoCompleteAsync([FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetTransactionTypeMasterAutoCompleteQuery(term ?? string.Empty));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateTransactionTypeMaster([FromBody] CreateTransactionTypeMasterCommand command)
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
        public async Task<IActionResult> UpdateTransactionTypeMaster([FromBody] UpdateTransactionTypeMasterCommand command)
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
        public async Task<IActionResult> DeleteTransactionTypeMaster(int id)
        {
            var result = await Mediator.Send(new DeleteTransactionTypeMasterCommand(id));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Transaction Type Master deleted successfully." : "Failed to delete Transaction Type Master."
            });
        }
    }
}
