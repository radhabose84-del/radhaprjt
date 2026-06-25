using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.BarcodeSeries.Command.CreateBarcodeSeries;
using PurchaseManagement.Application.BarcodeSeries.Command.DeleteBarcodeSeries;
using PurchaseManagement.Application.BarcodeSeries.Command.UpdateBarcodeSeries;
using PurchaseManagement.Application.BarcodeSeries.Queries.GetBarcodeSeries;
using PurchaseManagement.Application.BarcodeSeries.Queries.GetBarcodeSeriesAutoComplete;
using PurchaseManagement.Application.BarcodeSeries.Queries.GetBarcodeSeriesById;
using PurchaseManagement.Application.BarcodeSeries.Queries.GetBarcodeSeriesLabels;
using PurchaseManagement.Application.BarcodeSeries.Queries.GetNextBarcodeSeriesNumber;
using PurchaseManagement.Application.BarcodeSeries.Queries.GetNextBarcodeStartNumber;

namespace PurchaseManagement.Presentation.Controllers
{
    [Route("api/purchase/[controller]")]
    public class BarcodeSeriesController : ApiControllerBase
    {
        public BarcodeSeriesController(ISender mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllBarcodeSeriesAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetBarcodeSeriesQuery
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
            var result = await Mediator.Send(new GetBarcodeSeriesByIdQuery { Id = id });
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetAutoCompleteAsync([FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetBarcodeSeriesAutoCompleteQuery(term));
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result });
        }

        // Print-ready bale-barcode labels (letterhead + expanded barcode list + QR payload).
        [HttpGet("{id}/labels")]
        public async Task<IActionResult> GetLabelsAsync(int id)
        {
            var result = await Mediator.Send(new GetBarcodeSeriesLabelsQuery(id));
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result });
        }

        [HttpGet("next-number")]
        public async Task<IActionResult> GetNextNumberAsync([FromQuery] DateTimeOffset? generationDate = null)
        {
            var result = await Mediator.Send(new GetNextBarcodeSeriesNumberQuery
            {
                GenerationDate = generationDate ?? DateTimeOffset.Now
            });
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = new { nextNumber = result } });
        }

        [HttpGet("next-start")]
        public async Task<IActionResult> GetNextStartNumberAsync()
        {
            var result = await Mediator.Send(new GetNextBarcodeStartNumberQuery());
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = new { nextStartNumber = result } });
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] CreateBarcodeSeriesCommand command)
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
        public async Task<IActionResult> UpdateAsync([FromBody] UpdateBarcodeSeriesCommand command)
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
            var result = await Mediator.Send(new DeleteBarcodeSeriesCommand(id));
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result, message = "Deleted successfully." });
        }
    }
}
