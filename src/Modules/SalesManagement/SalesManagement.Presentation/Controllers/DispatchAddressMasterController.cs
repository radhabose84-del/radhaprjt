using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.DispatchAddressMaster.Commands.CreateDispatchAddressMaster;
using SalesManagement.Application.DispatchAddressMaster.Commands.DeleteDispatchAddressMaster;
using SalesManagement.Application.DispatchAddressMaster.Commands.UpdateDispatchAddressMaster;
using SalesManagement.Application.DispatchAddressMaster.Queries.GetAllDispatchAddressMaster;
using SalesManagement.Application.DispatchAddressMaster.Queries.GetDispatchAddressMasterAutoComplete;
using SalesManagement.Application.DispatchAddressMaster.Queries.GetDispatchAddressMasterById;

namespace SalesManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class DispatchAddressMasterController : ApiControllerBase
    {
        public DispatchAddressMasterController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllDispatchAddressMasterAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllDispatchAddressMasterQuery
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
        public async Task<IActionResult> GetDispatchAddressMasterByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetDispatchAddressMasterByIdQuery { Id = id });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetDispatchAddressMasterAutoCompleteAsync([FromQuery] string term = null!)
        {
            var result = await Mediator.Send(new GetDispatchAddressMasterAutoCompleteQuery(term));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateDispatchAddressMaster([FromBody] CreateDispatchAddressMasterCommand command)
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
        public async Task<IActionResult> UpdateDispatchAddressMaster([FromBody] UpdateDispatchAddressMasterCommand command)
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
        public async Task<IActionResult> DeleteDispatchAddressMaster(int id)
        {
            var result = await Mediator.Send(new DeleteDispatchAddressMasterCommand(id));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Dispatch Address Master deleted successfully." : "Dispatch Address Master not found."
            });
        }
    }
}
