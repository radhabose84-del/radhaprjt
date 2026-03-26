using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductionManagement.Application.YarnType.Commands.CreateYarnType;
using ProductionManagement.Application.YarnType.Commands.DeleteYarnType;
using ProductionManagement.Application.YarnType.Commands.UpdateYarnType;
using ProductionManagement.Application.YarnType.Queries.GetAllYarnType;
using ProductionManagement.Application.YarnType.Queries.GetYarnTypeAutoComplete;
using ProductionManagement.Application.YarnType.Queries.GetYarnTypeById;

namespace ProductionManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class YarnTypeController : ApiControllerBase
    {
        public YarnTypeController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllYarnTypeAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllYarnTypeQuery
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
        public async Task<IActionResult> GetYarnTypeByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetYarnTypeByIdQuery { Id = id });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetYarnTypeAutoCompleteAsync([FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetYarnTypeAutoCompleteQuery(term));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateYarnType([FromBody] CreateYarnTypeCommand command)
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
        public async Task<IActionResult> UpdateYarnType([FromBody] UpdateYarnTypeCommand command)
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
        public async Task<IActionResult> DeleteYarnType(int id)
        {
            var result = await Mediator.Send(new DeleteYarnTypeCommand(id));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Yarn Type deleted successfully." : "Yarn Type not found."
            });
        }
    }
}
