using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.MovementTypeConfig.Commands.CreateMovementTypeConfig;
using SalesManagement.Application.MovementTypeConfig.Commands.UpdateMovementTypeConfig;
using SalesManagement.Application.MovementTypeConfig.Commands.DeleteMovementTypeConfig;
using SalesManagement.Application.MovementTypeConfig.Queries.GetAllMovementTypeConfig;
using SalesManagement.Application.MovementTypeConfig.Queries.GetMovementTypeConfigById;
using SalesManagement.Application.MovementTypeConfig.Queries.GetMovementTypeConfigAutoComplete;

namespace SalesManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class MovementTypeConfigController : ApiControllerBase
    {
        public MovementTypeConfigController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllMovementTypeConfigAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllMovementTypeConfigQuery
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
        public async Task<IActionResult> GetMovementTypeConfigByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetMovementTypeConfigByIdQuery { Id = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetMovementTypeConfigAutoCompleteAsync([FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetMovementTypeConfigAutoCompleteQuery(term ?? string.Empty));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateMovementTypeConfig([FromBody] CreateMovementTypeConfigCommand command)
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
        public async Task<IActionResult> UpdateMovementTypeConfig([FromBody] UpdateMovementTypeConfigCommand command)
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
        public async Task<IActionResult> DeleteMovementTypeConfig(int id)
        {
            var result = await Mediator.Send(new DeleteMovementTypeConfigCommand(id));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Movement Type Configuration deleted successfully." : "Failed to delete Movement Type Configuration."
            });
        }
    }
}
