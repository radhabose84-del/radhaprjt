using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.OCREntry.Commands.CreateOCREntry;
using PurchaseManagement.Application.OCREntry.Commands.DeleteDocument;
using PurchaseManagement.Application.OCREntry.Commands.DeleteOCREntry;
using PurchaseManagement.Application.OCREntry.Commands.UpdateOCREntry;
using PurchaseManagement.Application.OCREntry.Commands.UploadDocument;
using PurchaseManagement.Application.OCREntry.Queries.GetAllOCREntry;
using PurchaseManagement.Application.OCREntry.Queries.GetOCREntryAutoComplete;
using PurchaseManagement.Application.OCREntry.Queries.GetNextOcrNumber;
using PurchaseManagement.Application.OCREntry.Queries.GetOCREntryById;
using PurchaseManagement.Application.OCREntry.Queries.GetOCREntryPending;
using PurchaseManagement.Application.OCREntry.Queries.GetOCREntryReport;
using PurchaseManagement.Application.OCREntry.Queries.GetOCRQualityTemplateParameters;

namespace PurchaseManagement.Presentation.Controllers
{
    public class OCREntryController : ApiControllerBase
    {
        public OCREntryController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllOCREntryAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null,
            [FromQuery] int? StatusId = null,
            [FromQuery] DateTimeOffset? FromDate = null,
            [FromQuery] DateTimeOffset? ToDate = null)
        {
            var result = await Mediator.Send(new GetAllOCREntryQuery
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                SearchTerm = SearchTerm,
                StatusId = StatusId,
                FromDate = FromDate,
                ToDate = ToDate
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

        // Dynamic Order Confirmation Report payload (sectioned key/label/value) for printing.
        [HttpGet("{id}/report")]
        public async Task<IActionResult> GetOCREntryReportAsync(int id)
        {
            var result = await Mediator.Send(new GetOCREntryReportQuery(id));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetOCREntryAutoCompleteAsync([FromQuery] string term = "", [FromQuery] bool approved = true, [FromQuery] bool showAll = false)
        {
            var result = await Mediator.Send(new GetOCREntryAutoCompleteQuery(term, approved, showAll));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        // Returns the active parameters of a quality template so the OCR form can render the
        // cotton-quality parameter inputs dynamically.
        [HttpGet("quality-template/{templateId}/parameters")]
        public async Task<IActionResult> GetQualityTemplateParametersAsync(int templateId)
        {
            var result = await Mediator.Send(new GetOCRQualityTemplateParametersQuery(templateId));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        // Peek the last-issued + next OCR number for the OCR Management screen (no increment).
        [HttpGet("next-number")]
        public async Task<IActionResult> GetNextOcrNumberAsync()
        {
            var result = await Mediator.Send(new GetNextOcrNumberQuery());
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

        // UPLOAD DOCUMENT (multipart) — stages one file, returns metadata for inclusion in CREATE body
        [HttpPost("upload-document")]
        public async Task<IActionResult> UploadDocument([FromForm] UploadOCRDocumentCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = true,
                message = "Document staged successfully.",
                data = result
            });
        }

        // DELETE DOCUMENT — deletes a document by its file name (e.g. "TEMP_...png" or "OCR-2026-0001.png")
        [HttpDelete("document")]
        public async Task<IActionResult> DeleteDocument([FromQuery] string fileName)
        {
            var result = await Mediator.Send(new DeleteOCRDocumentCommand(fileName));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Document deleted successfully." : "Document not found."
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
