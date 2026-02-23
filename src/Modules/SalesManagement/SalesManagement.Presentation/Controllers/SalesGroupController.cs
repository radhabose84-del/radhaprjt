#nullable disable
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.SalesGroup.Commands.CreateSalesGroup;
using SalesManagement.Application.SalesGroup.Commands.UpdateSalesGroup;
using SalesManagement.Application.SalesGroup.Commands.DeleteSalesGroup;
using SalesManagement.Application.SalesGroup.Queries.GetAllSalesGroup;
using SalesManagement.Application.SalesGroup.Queries.GetSalesGroupById;
using SalesManagement.Application.SalesGroup.Queries.GetSalesGroupAutoComplete;

namespace SalesManagement.Presentation.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class SalesGroupController : ApiControllerBase
    {
        public SalesGroupController(IMediator mediator) : base(mediator)
        {
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSalesGroupAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllSalesGroupQuery
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
        public async Task<IActionResult> GetSalesGroupByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetSalesGroupByIdQuery { Id = id });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result.Data,
                message = result.Message,
                isSuccess = result.IsSuccess
            });
        }

        [HttpGet("autocomplete")]
        public async Task<IActionResult> GetSalesGroupAutoCompleteAsync(
            [FromQuery] string term = null)
        {
            var result = await Mediator.Send(new GetSalesGroupAutoCompleteQuery(term ?? string.Empty));
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result });
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateSalesGroup([FromBody] CreateSalesGroupCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await Mediator.Send(command);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result.IsSuccess,
                message = result.Message,
                data = result.Data
            });
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateSalesGroup([FromBody] UpdateSalesGroupCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (command.Id <= 0)
                return BadRequest(new { StatusCode = StatusCodes.Status400BadRequest, Message = "Invalid Id provided." });

            var result = await Mediator.Send(command);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result.IsSuccess,
                message = result.Message,
                data = result.Data
            });
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteSalesGroup(int id)
        {
            if (id <= 0)
                return BadRequest(new { StatusCode = StatusCodes.Status400BadRequest, Message = "Invalid Id provided." });

            var result = await Mediator.Send(new DeleteSalesGroupCommand(id));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Sales Group deleted successfully." : "Failed to delete Sales Group.",
                data = result
            });
        }
    }
}
