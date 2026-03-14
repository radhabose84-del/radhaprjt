using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductionManagement.Application.CountMaster.Commands.CreateCountMaster;
using ProductionManagement.Application.CountMaster.Commands.DeleteCountMaster;
using ProductionManagement.Application.CountMaster.Commands.UpdateCountMaster;
using ProductionManagement.Application.CountMaster.Queries.GetAllCountMaster;
using ProductionManagement.Application.CountMaster.Queries.GetCountMasterAutoComplete;
using ProductionManagement.Application.CountMaster.Queries.GetCountMasterById;

namespace ProductionManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class CountMasterController : ApiControllerBase
    {
        public CountMasterController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllCountMasterAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllCountMasterQuery
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
        public async Task<IActionResult> GetCountMasterByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetCountMasterByIdQuery { Id = id });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetCountMasterAutoCompleteAsync([FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetCountMasterAutoCompleteQuery(term));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateCountMaster([FromBody] CreateCountMasterCommand command)
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
        public async Task<IActionResult> UpdateCountMaster([FromBody] UpdateCountMasterCommand command)
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
        public async Task<IActionResult> DeleteCountMaster(int id)
        {
            var result = await Mediator.Send(new DeleteCountMasterCommand(id));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Count Master deleted successfully." : "Count Master not found."
            });
        }
    }
}
