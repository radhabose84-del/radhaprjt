using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.RawMaterialPO.Commands.CreateRawMaterialPO;
using PurchaseManagement.Application.RawMaterialPO.Commands.DeleteDocument;
using PurchaseManagement.Application.RawMaterialPO.Commands.DeleteRawMaterialPO;
using PurchaseManagement.Application.RawMaterialPO.Commands.UpdateRawMaterialPO;
using PurchaseManagement.Application.RawMaterialPO.Commands.UploadDocument;
using PurchaseManagement.Application.RawMaterialPO.Queries.GetAllRawMaterialPO;
using PurchaseManagement.Application.RawMaterialPO.Queries.GetRawMaterialPOAutoComplete;
using PurchaseManagement.Application.RawMaterialPO.Queries.GetRawMaterialPOById;
using PurchaseManagement.Application.RawMaterialPO.Queries.GetRawMaterialPOFromOcr;

namespace PurchaseManagement.Presentation.Controllers
{
    public class RawMaterialPOController : ApiControllerBase
    {
        public RawMaterialPOController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllRawMaterialPOAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null,
            [FromQuery] DateTimeOffset? FromDate = null,
            [FromQuery] DateTimeOffset? ToDate = null)
        {
            var result = await Mediator.Send(new GetAllRawMaterialPOQuery
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                SearchTerm = SearchTerm,
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
        public async Task<IActionResult> GetRawMaterialPOByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetRawMaterialPOByIdQuery { Id = id });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetRawMaterialPOAutoCompleteAsync([FromQuery] string term = "", [FromQuery] bool showAll = false)
        {
            var result = await Mediator.Send(new GetRawMaterialPOAutoCompleteQuery(term, showAll));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        // Auto-fetch approved-OCR details + RemainingQuantity ("Max N Bales") for the conversion screen
        [HttpGet("from-ocr/{ocrId}")]
        public async Task<IActionResult> GetRawMaterialPOFromOcrAsync(int ocrId)
        {
            var result = await Mediator.Send(new GetRawMaterialPOFromOcrQuery { OcrId = ocrId });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateRawMaterialPO([FromBody] CreateRawMaterialPOCommand command)
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

        // UPLOAD DOCUMENT (multipart) — stages one file, returns metadata for inclusion in CREATE/UPDATE body
        [HttpPost("upload-document")]
        public async Task<IActionResult> UploadDocument([FromForm] UploadRawMaterialPODocumentCommand command)
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

        // DELETE DOCUMENT — deletes a document by its file name (e.g. "TEMP_...png" or "{PONumber}.png")
        [HttpDelete("document")]
        public async Task<IActionResult> DeleteDocument([FromQuery] string fileName)
        {
            var result = await Mediator.Send(new DeleteRawMaterialPODocumentCommand(fileName));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Document deleted successfully." : "Document not found."
            });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateRawMaterialPO([FromBody] UpdateRawMaterialPOCommand command)
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
        public async Task<IActionResult> DeleteRawMaterialPO(int id)
        {
            var result = await Mediator.Send(new DeleteRawMaterialPOCommand(id));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Raw Material PO deleted successfully." : "Raw Material PO not found."
            });
        }
    }
}
