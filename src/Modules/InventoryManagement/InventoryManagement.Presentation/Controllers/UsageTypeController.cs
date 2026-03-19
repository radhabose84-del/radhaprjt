using InventoryManagement.Application.UsageType.Commands.CreateUsageType;
using InventoryManagement.Application.UsageType.Commands.DeleteUsageType;
using InventoryManagement.Application.UsageType.Commands.UpdateUsageType;
using InventoryManagement.Application.UsageType.Queries.GetAllUsageType;
using InventoryManagement.Application.UsageType.Queries.GetUsageTypeAutoComplete;
using InventoryManagement.Application.UsageType.Queries.GetUsageTypeById;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class UsageTypeController : ApiControllerBase
    {
        public UsageTypeController(ISender mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllUsageTypeAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllUsageTypeQuery
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
        public async Task<IActionResult> GetUsageTypeByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetUsageTypeByIdQuery { Id = id });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetUsageTypeAutoCompleteAsync([FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetUsageTypeAutoCompleteQuery(term));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateUsageType([FromBody] CreateUsageTypeCommand command)
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
        public async Task<IActionResult> UpdateUsageType([FromBody] UpdateUsageTypeCommand command)
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
        public async Task<IActionResult> DeleteUsageType(int id)
        {
            var result = await Mediator.Send(new DeleteUsageTypeCommand(id));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "UsageType deleted successfully." : "UsageType not found."
            });
        }
    }
}
