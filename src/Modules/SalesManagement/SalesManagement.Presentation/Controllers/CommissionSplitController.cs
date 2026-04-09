using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.CommissionSplit.Commands.CreateCommissionSplit;
using SalesManagement.Application.CommissionSplit.Commands.DeleteCommissionSplit;
using SalesManagement.Application.CommissionSplit.Commands.UpdateCommissionSplit;
using SalesManagement.Application.CommissionSplit.Queries.GetAllCommissionSplit;
using SalesManagement.Application.CommissionSplit.Queries.GetCommissionSplitAutoComplete;
using SalesManagement.Application.CommissionSplit.Queries.GetCommissionSplitById;

namespace SalesManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class CommissionSplitController : ApiControllerBase
    {
        public CommissionSplitController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllCommissionSplitAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllCommissionSplitQuery
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                SearchTerm = SearchTerm
            });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result.Data,
                result.TotalCount,
                result.PageNumber,
                result.PageSize
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCommissionSplitByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetCommissionSplitByIdQuery { Id = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetCommissionSplitAutoCompleteAsync([FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetCommissionSplitAutoCompleteQuery(term ?? string.Empty));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateCommissionSplit([FromBody] CreateCommissionSplitCommand command)
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
        public async Task<IActionResult> UpdateCommissionSplit([FromBody] UpdateCommissionSplitCommand command)
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
        public async Task<IActionResult> DeleteCommissionSplit(int id)
        {
            var result = await Mediator.Send(new DeleteCommissionSplitCommand(id));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "CommissionSplit deleted successfully." : "Failed to delete CommissionSplit."
            });
        }
    }
}
