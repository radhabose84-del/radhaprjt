using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductionManagement.Application.Repacking.Commands.CreateRepacking;
using ProductionManagement.Application.Repacking.Commands.DeleteRepacking;
using ProductionManagement.Application.Repacking.Commands.UpdateRepacking;
using ProductionManagement.Application.Repacking.Queries.GetAllRepacking;
using ProductionManagement.Application.Repacking.Queries.GetRepackingAutoComplete;
using ProductionManagement.Application.Repacking.Queries.GetRepackingById;

namespace ProductionManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class RepackingController : ApiControllerBase
    {
        public RepackingController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllRepackingAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllRepackingQuery
            {
                PageNumber = PageNumber,
                PageSize   = PageSize,
                SearchTerm = SearchTerm
            });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data       = result.Data,
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize   = result.PageSize
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRepackingByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetRepackingByIdQuery { Id = id });
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetRepackingAutoCompleteAsync([FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetRepackingAutoCompleteQuery(term));
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result });
        }

        [HttpPost]
        public async Task<IActionResult> CreateRepacking([FromBody] CreateRepackingCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess  = result.IsSuccess,
                message    = result.Message,
                data       = result.Data
            });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateRepacking([FromBody] UpdateRepackingCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess  = result.IsSuccess,
                message    = result.Message,
                data       = result.Data
            });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteRepacking(int id)
        {
            var result = await Mediator.Send(new DeleteRepackingCommand(id));
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result });
        }
    }
}
