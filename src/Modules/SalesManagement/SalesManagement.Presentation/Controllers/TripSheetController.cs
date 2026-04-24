using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.TripSheet.Commands.CreateTripSheet;
using SalesManagement.Application.TripSheet.Commands.UpdateTripSheet;
using SalesManagement.Application.TripSheet.Commands.DeleteTripSheet;
using SalesManagement.Application.TripSheet.Queries.GetAllTripSheet;
using SalesManagement.Application.TripSheet.Queries.GetTripSheetById;
using SalesManagement.Application.TripSheet.Queries.GetTripSheetAutoComplete;
using SalesManagement.Application.TripSheet.Queries.GetTripSheetDispatchPackingList;
using SalesManagement.Application.TripSheet.Queries.GetTripSheetInvoicePrintDetails;

namespace SalesManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class TripSheetController : ApiControllerBase
    {
        public TripSheetController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllTripSheetAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllTripSheetQuery
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
        public async Task<IActionResult> GetTripSheetByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetTripSheetByIdQuery { Id = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetTripSheetAutoCompleteAsync([FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetTripSheetAutoCompleteQuery(term ?? string.Empty));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateTripSheet([FromBody] CreateTripSheetCommand command)
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
        public async Task<IActionResult> UpdateTripSheet([FromBody] UpdateTripSheetCommand command)
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
        public async Task<IActionResult> DeleteTripSheet(int id)
        {
            var result = await Mediator.Send(new DeleteTripSheetCommand(id));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = "TripSheet deleted successfully."
            });
        }

        [HttpGet("{id}/packing-list")]
        public async Task<IActionResult> GetPackingListAsync(int id)
        {
            var result = await Mediator.Send(new GetTripSheetDispatchPackingListQuery { TripSheetHeaderId = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("{id}/invoices")]
        public async Task<IActionResult> GetInvoicePrintAsync(int id)
        {
            var result = await Mediator.Send(new GetTripSheetInvoicePrintDetailsQuery { TripSheetHeaderId = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }
    }
}
