using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.ItemPriceMaster.Commands.CreateItemPriceMaster;
using SalesManagement.Application.ItemPriceMaster.Commands.UpdateItemPriceMaster;
using SalesManagement.Application.ItemPriceMaster.Commands.DeleteItemPriceMaster;
using SalesManagement.Application.ItemPriceMaster.Queries.GetAllItemPriceMaster;
using SalesManagement.Application.ItemPriceMaster.Queries.GetItemPriceMasterById;
using SalesManagement.Application.ItemPriceMaster.Queries.GetItemPriceByItemAndDate;
using SalesManagement.Application.ItemPriceMaster.Queries.GetItemPriceMasterAutoComplete;
using SalesManagement.Application.ItemPriceMaster.Queries.GetExMillRateByPaymentTerm;

namespace SalesManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class ItemPriceMasterController : ApiControllerBase
    {
        public ItemPriceMasterController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllItemPriceMasterAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllItemPriceMasterQuery
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
        public async Task<IActionResult> GetItemPriceMasterByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetItemPriceMasterByIdQuery { Id = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-item-date")]
        public async Task<IActionResult> GetItemPriceByItemAndDateAsync(
            [FromQuery] int ItemId,
            [FromQuery] DateOnly Date)
        {
            var result = await Mediator.Send(new GetItemPriceByItemAndDateQuery
            {
                ItemId = ItemId,
                Date = Date
            });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result.Data,
                TotalCount = result.TotalCount
            });
        }

        [HttpGet("exmill-rate")]
        public async Task<IActionResult> GetExMillRateByPaymentTermAsync(
            [FromQuery] int itemId,
            [FromQuery] DateOnly date,
            [FromQuery] int? paymentTermId = null,
            [FromQuery] int? salesSegmentId = null)
        {
            var result = await Mediator.Send(new GetExMillRateByPaymentTermQuery
            {
                PaymentTermId = paymentTermId,
                ItemId = itemId,
                Date = date,
                SalesSegmentId = salesSegmentId
            });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result.Data,
                TotalCount = result.TotalCount
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetItemPriceMasterAutoCompleteAsync([FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetItemPriceMasterAutoCompleteQuery(term ?? string.Empty));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateItemPriceMaster([FromBody] CreateItemPriceMasterCommand command)
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
        public async Task<IActionResult> UpdateItemPriceMaster([FromBody] UpdateItemPriceMasterCommand command)
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
        public async Task<IActionResult> DeleteItemPriceMaster(int id)
        {
            var result = await Mediator.Send(new DeleteItemPriceMasterCommand(id));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Item Price Master deleted successfully." : "Failed to delete Item Price Master."
            });
        }
    }
}
