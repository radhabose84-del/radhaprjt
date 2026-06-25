using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.BarcodeAllocation.Command.CreateBarcodeAllocation;
using PurchaseManagement.Application.BarcodeAllocation.Command.DeleteBarcodeAllocation;
using PurchaseManagement.Application.BarcodeAllocation.Command.UpdateBarcodeAllocation;
using PurchaseManagement.Application.BarcodeAllocation.Queries.GetAllocationBarcodeSeries;
using PurchaseManagement.Application.BarcodeAllocation.Queries.GetAllocationEmployees;
using PurchaseManagement.Application.BarcodeAllocation.Queries.GetBarcodeAllocation;
using PurchaseManagement.Application.BarcodeAllocation.Queries.GetBarcodeAllocationAutoComplete;
using PurchaseManagement.Application.BarcodeAllocation.Queries.GetBarcodeAllocationById;
using PurchaseManagement.Application.BarcodeAllocation.Queries.GetNextAllocationFrom;
using PurchaseManagement.Application.BarcodeAllocation.Queries.GetNextAllocationNumber;

namespace PurchaseManagement.Presentation.Controllers
{
    [Route("api/purchase/[controller]")]
    public class BarcodeAllocationController : ApiControllerBase
    {
        public BarcodeAllocationController(ISender mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllBarcodeAllocationAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetBarcodeAllocationQuery
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
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetBarcodeAllocationByIdQuery { Id = id });
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetAutoCompleteAsync([FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetBarcodeAllocationAutoCompleteQuery(term));
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result });
        }

        [HttpGet("employees")]
        public async Task<IActionResult> GetEmployeesAsync([FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetAllocationEmployeesQuery(term));
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result });
        }

        [HttpGet("barcode-series")]
        public async Task<IActionResult> GetBarcodeSeriesAsync([FromQuery] string? term = null, [FromQuery] int? seriesId = null)
        {
            var result = await Mediator.Send(new GetAllocationBarcodeSeriesQuery(term, seriesId));
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result });
        }

        [HttpGet("next-number")]
        public async Task<IActionResult> GetNextNumberAsync([FromQuery] DateTimeOffset? allocationDate = null)
        {
            var result = await Mediator.Send(new GetNextAllocationNumberQuery
            {
                AllocationDate = allocationDate ?? DateTimeOffset.Now
            });
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = new { nextNumber = result } });
        }

        [HttpGet("next-from")]
        public async Task<IActionResult> GetNextFromAsync([FromQuery] int seriesId)
        {
            var result = await Mediator.Send(new GetNextAllocationFromQuery { BarcodeSeriesId = seriesId });
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = new { nextFrom = result } });
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] CreateBarcodeAllocationCommand command)
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
        public async Task<IActionResult> UpdateAsync([FromBody] UpdateBarcodeAllocationCommand command)
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var result = await Mediator.Send(new DeleteBarcodeAllocationCommand(id));
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result, message = "Deleted successfully." });
        }
    }
}
