using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.OCREntry.Commands.CreateOCREntry;
using PurchaseManagement.Application.OCREntry.Commands.DeleteOCREntry;
using PurchaseManagement.Application.OCREntry.Commands.UpdateOCREntry;
using PurchaseManagement.Application.OCREntry.Queries.GetAllOCREntry;
using PurchaseManagement.Application.OCREntry.Queries.GetOCREntryAutoComplete;
using PurchaseManagement.Application.OCREntry.Queries.GetOCREntryById;
using PurchaseManagement.Application.OCREntry.Queries.GetOCREntryPending;

namespace PurchaseManagement.Presentation.Controllers
{
    public class OCREntryController : ApiControllerBase
    {
        public OCREntryController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllOCREntryAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllOCREntryQuery
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
        public async Task<IActionResult> GetOCREntryByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetOCREntryByIdQuery { Id = id });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetOCREntryAutoCompleteAsync([FromQuery] string term = "")
        {
            var result = await Mediator.Send(new GetOCREntryAutoCompleteQuery(term));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetOCREntryPendingAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize)
        {
            var result = await Mediator.Send(new GetOCREntryPendingQuery
            {
                PageNumber = PageNumber,
                PageSize = PageSize
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

        [HttpPost]
        public async Task<IActionResult> CreateOCREntry([FromBody] CreateOCREntryCommand command)
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
        public async Task<IActionResult> UpdateOCREntry([FromBody] UpdateOCREntryCommand command)
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
        public async Task<IActionResult> DeleteOCREntry(int id)
        {
            var result = await Mediator.Send(new DeleteOCREntryCommand(id));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "OCR deleted successfully." : "OCR not found."
            });
        }
    }
}
