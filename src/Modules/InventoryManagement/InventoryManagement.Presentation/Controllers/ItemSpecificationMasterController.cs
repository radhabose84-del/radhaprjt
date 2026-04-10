using InventoryManagement.Application.ItemSpecificationMaster.Commands.CreateItemSpecificationMaster;
using InventoryManagement.Application.ItemSpecificationMaster.Commands.DeleteItemSpecificationMaster;
using InventoryManagement.Application.ItemSpecificationMaster.Commands.UpdateItemSpecificationMaster;
using InventoryManagement.Application.ItemSpecificationMaster.Queries.GetAllItemSpecificationMaster;
using InventoryManagement.Application.ItemSpecificationMaster.Queries.GetItemSpecificationMasterAutoComplete;
using InventoryManagement.Application.ItemSpecificationMaster.Queries.GetItemSpecificationMasterById;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class ItemSpecificationMasterController : ApiControllerBase
    {
        public ItemSpecificationMasterController(ISender mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllItemSpecificationMasterAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllItemSpecificationMasterQuery
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
        public async Task<IActionResult> GetItemSpecificationMasterByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetItemSpecificationMasterByIdQuery { Id = id });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetItemSpecificationMasterAutoCompleteAsync([FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetItemSpecificationMasterAutoCompleteQuery(term ?? string.Empty));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateItemSpecificationMaster([FromBody] CreateItemSpecificationMasterCommand command)
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
        public async Task<IActionResult> UpdateItemSpecificationMaster([FromBody] UpdateItemSpecificationMasterCommand command)
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
        public async Task<IActionResult> DeleteItemSpecificationMaster(int id)
        {
            var result = await Mediator.Send(new DeleteItemSpecificationMasterCommand(id));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "ItemSpecificationMaster deleted successfully." : "ItemSpecificationMaster not found."
            });
        }
    }
}
