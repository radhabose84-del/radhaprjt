using InventoryManagement.Application.ItemSpecificationValue.Commands.CreateItemSpecificationValue;
using InventoryManagement.Application.ItemSpecificationValue.Commands.DeleteItemSpecificationValue;
using InventoryManagement.Application.ItemSpecificationValue.Commands.UpdateItemSpecificationValue;
using InventoryManagement.Application.ItemSpecificationValue.Queries.GetAllItemSpecificationValue;
using InventoryManagement.Application.ItemSpecificationValue.Queries.GetItemSpecificationValueAutoComplete;
using InventoryManagement.Application.ItemSpecificationValue.Queries.GetItemSpecificationValueById;
using InventoryManagement.Application.ItemSpecificationValue.Queries.GetItemSpecificationValueBySpecificationMaster;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class ItemSpecificationValueController : ApiControllerBase
    {
        public ItemSpecificationValueController(ISender mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllItemSpecificationValueAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllItemSpecificationValueQuery
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
        public async Task<IActionResult> GetItemSpecificationValueByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetItemSpecificationValueByIdQuery { Id = id });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetItemSpecificationValueAutoCompleteAsync(
            [FromQuery] string? term = null,
            [FromQuery] int? specificationMasterId = null)
        {
            var result = await Mediator.Send(
                new GetItemSpecificationValueAutoCompleteQuery(term ?? string.Empty, specificationMasterId));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-specification-master/{specificationMasterId}")]
        public async Task<IActionResult> GetBySpecificationMasterIdAsync(int specificationMasterId)
        {
            var result = await Mediator.Send(new GetItemSpecificationValueBySpecificationMasterQuery(specificationMasterId));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result.Data,
                TotalCount = result.TotalCount
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateItemSpecificationValue([FromBody] CreateItemSpecificationValueCommand command)
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
        public async Task<IActionResult> UpdateItemSpecificationValue([FromBody] UpdateItemSpecificationValueCommand command)
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
        public async Task<IActionResult> DeleteItemSpecificationValue(int id)
        {
            var result = await Mediator.Send(new DeleteItemSpecificationValueCommand(id));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "ItemSpecificationValue deleted successfully." : "ItemSpecificationValue not found."
            });
        }
    }
}
