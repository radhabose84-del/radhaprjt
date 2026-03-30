using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductionManagement.Application.CountGroup.Commands.CreateCountGroup;
using ProductionManagement.Application.CountGroup.Commands.DeleteCountGroup;
using ProductionManagement.Application.CountGroup.Commands.UpdateCountGroup;
using ProductionManagement.Application.CountGroup.Queries.GetAllCountGroup;
using ProductionManagement.Application.CountGroup.Queries.GetCountGroupAutoComplete;
using ProductionManagement.Application.CountGroup.Queries.GetCountGroupById;

namespace ProductionManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class CountGroupController : ApiControllerBase
    {
        public CountGroupController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllCountGroupAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllCountGroupQuery
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
        public async Task<IActionResult> GetCountGroupByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetCountGroupByIdQuery { Id = id });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetCountGroupAutoCompleteAsync([FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetCountGroupAutoCompleteQuery(term));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateCountGroup([FromBody] CreateCountGroupCommand command)
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
        public async Task<IActionResult> UpdateCountGroup([FromBody] UpdateCountGroupCommand command)
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
        public async Task<IActionResult> DeleteCountGroup(int id)
        {
            var result = await Mediator.Send(new DeleteCountGroupCommand(id));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Count Group deleted successfully." : "Count Group not found."
            });
        }
    }
}
