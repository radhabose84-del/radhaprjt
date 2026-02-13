using PurchaseManagement.Application.PurchaseOrder.DeletePODocument;
using PurchaseManagement.Application.PurchaseOrder.Dtos.ImportPO;
using PurchaseManagement.Application.PurchaseOrder.ImportPO.Command.Create;
using PurchaseManagement.Application.PurchaseOrder.ImportPO.Command.ImportPOAmendment;
using PurchaseManagement.Application.PurchaseOrder.ImportPO.Queries.GetImportPOPending;
using PurchaseManagement.Application.PurchaseOrder.ImportPO.Queries.GetPOById;
using PurchaseManagement.Application.PurchaseOrder.UploadPODocument;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace PurchaseManagement.Presentation.Controllers.PurchaseOrder
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImportPOController : ApiControllerBase
    {
        public ImportPOController(ISender mediator) : base(mediator)
        {

        }

        // GET: api/ImportPO/123
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var data = await Mediator.Send(new GetImportPOByIdQuery(id), ct);
            return Ok(new { StatusCode = data is null ? 404 : 200, message = data is null ? "Not found" : "Fetched", data });
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] ImportPOCreateDto dto, CancellationToken ct = default)
        {
            var id = await Mediator.Send(new CreateImportPOCommand
            { Data = dto }, ct);

            return Ok(new
            {
                StatusCode = StatusCodes.Status201Created,
                message = "Created successfully.",
                data = id
            });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAsync([FromBody] ImportPOUpdateDto dto, CancellationToken ct = default)
        {
            var ok = await Mediator.Send(new UpdateImportPOCommand
            {
                Data = dto
            }, ct);
            return Ok(new
            {
                StatusCode = ok ? StatusCodes.Status200OK : StatusCodes.Status404NotFound,
                message = ok ? "Updated successfully." : "Not found.",
                data = ok
            });
        }
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingImportPO(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? searchTerm = null,
            [FromQuery] int? poId = null,
            CancellationToken ct = default)
        {
            var (items, total) = await Mediator.Send(new GetImportPOsPendingQuery
            {
                PoId = poId,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm
            }, ct);

            if (items == null || items.Count == 0)
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    data = (object?)null,
                    message = $"Pending PO data for PO ID {poId} not found"
                });
            }
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = new { Items = items, TotalCount = total },
                message = $"Pending PO data for PO ID {poId} fetched successfully"
            });
        }
        [HttpPost("amendment")]
        public async Task<IActionResult> Amend([FromBody] ImportPOUpdateDto dto, CancellationToken ct)
        {
            if (dto is null)
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = "Invalid payload: id mismatch.",
                    data = (object?)null
                });

            var newId = await Mediator.Send(new ImportPOAmendmentCommand { Data = dto }, ct);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "Amendment created successfully.",
                data = new { NewPurchaseOrderId = newId }
            });
        }
        [HttpPost("upload-document")]
        public async Task<IActionResult> UploadDocument(UploadPODocumentCommand uploadFileCommand)
        {
            var file = await Mediator.Send(uploadFileCommand);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "File uploaded successfully.",
                data = file,
                errors = ""
            });
        }
           [HttpDelete("delete-document")]
        public async Task<IActionResult> DeleteDocument([FromBody] DeletePODocumentCommand deleteFileCommand)
        {
            if (deleteFileCommand == null || string.IsNullOrWhiteSpace(deleteFileCommand.PODocumentPath))
            {
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = "Invalid request. 'PODocumentPath' cannot be null or empty.",
                    errors = ""
                });
            }
            var file = await Mediator.Send(deleteFileCommand);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "File deleted successfully.",
                errors = ""
            });
        }
    }
}
