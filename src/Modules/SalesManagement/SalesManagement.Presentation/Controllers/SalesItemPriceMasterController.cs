#nullable disable
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.SalesItemPriceMaster.Commands.CreateSalesItemPriceMaster;
using SalesManagement.Application.SalesItemPriceMaster.Commands.UpdateSalesItemPriceMaster;
using SalesManagement.Application.SalesItemPriceMaster.Commands.DeleteSalesItemPriceMaster;
using SalesManagement.Application.SalesItemPriceMaster.Queries.GetAllSalesItemPriceMaster;
using SalesManagement.Application.SalesItemPriceMaster.Queries.GetSalesItemPriceMasterById;
using SalesManagement.Application.SalesItemPriceMaster.Queries.GetSalesItemPriceMasterAutoComplete;

namespace SalesManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class SalesItemPriceMasterController : ApiControllerBase
    {
        public SalesItemPriceMasterController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllSalesItemPriceMasterAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllSalesItemPriceMasterQuery
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
        public async Task<IActionResult> GetSalesItemPriceMasterByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetSalesItemPriceMasterByIdQuery { Id = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result.IsSuccess,
                message = result.Message,
                data = result.Data
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetSalesItemPriceMasterAutoCompleteAsync([FromQuery] string term = null)
        {
            var result = await Mediator.Send(new GetSalesItemPriceMasterAutoCompleteQuery(term ?? string.Empty));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateSalesItemPriceMaster([FromBody] CreateSalesItemPriceMasterCommand command)
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
        public async Task<IActionResult> UpdateSalesItemPriceMaster([FromBody] UpdateSalesItemPriceMasterCommand command)
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
        public async Task<IActionResult> DeleteSalesItemPriceMaster(int id)
        {
            var result = await Mediator.Send(new DeleteSalesItemPriceMasterCommand(id));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Sales Item Price Master deleted successfully." : "Failed to delete Sales Item Price Master."
            });
        }
    }
}
