using InventoryManagement.Application.PriceGroupMaster.Commands.CreatePriceGroupMaster;
using InventoryManagement.Application.PriceGroupMaster.Commands.DeletePriceGroupMaster;
using InventoryManagement.Application.PriceGroupMaster.Commands.UpdatePriceGroupMaster;
using InventoryManagement.Application.PriceGroupMaster.Queries.GetAllPriceGroupMaster;
using InventoryManagement.Application.PriceGroupMaster.Queries.GetPriceGroupMasterAutoComplete;
using InventoryManagement.Application.PriceGroupMaster.Queries.GetPriceGroupMasterById;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class PriceGroupMasterController : ApiControllerBase
    {
        public PriceGroupMasterController(IMediator mediator) : base(mediator)
        {
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPriceGroupMasterAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllPriceGroupMasterQuery
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
        public async Task<IActionResult> GetPriceGroupMasterByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetPriceGroupMasterByIdQuery { Id = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetPriceGroupMasterAutoCompleteAsync([FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetPriceGroupMasterAutoCompleteQuery(term ?? string.Empty));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreatePriceGroupMaster([FromBody] CreatePriceGroupMasterCommand command)
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
        public async Task<IActionResult> UpdatePriceGroupMaster([FromBody] UpdatePriceGroupMasterCommand command)
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
        public async Task<IActionResult> DeletePriceGroupMaster([FromQuery] int id)
        {
            var result = await Mediator.Send(new DeletePriceGroupMasterCommand(id));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Price Group deleted successfully." : "Price Group not found."
            });
        }
    }
}
