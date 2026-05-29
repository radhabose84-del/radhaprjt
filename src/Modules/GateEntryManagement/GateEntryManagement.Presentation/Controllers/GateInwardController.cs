using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using GateEntryManagement.Application.GateInward.Commands.CreateGateInward;
using GateEntryManagement.Application.GateInward.Commands.DeleteGateInward;
using GateEntryManagement.Application.GateInward.Commands.UploadGateInwardAttachment;
using GateEntryManagement.Application.GateInward.Commands.DeleteGateInwardAttachment;
using GateEntryManagement.Application.GateInward.Queries.GetAllGateInward;
using GateEntryManagement.Application.GateInward.Queries.GetGateInwardById;
using GateEntryManagement.Application.GateInward.Queries.GetGateInwardAutoComplete;
using GateEntryManagement.Application.GateInward.Queries.GetPendingReferenceDocs;
using GateEntryManagement.Application.GateInward.Queries.GetPendingReferenceDocItems;

namespace GateEntryManagement.Presentation.Controllers
{
    [Route("api/gateentry/[controller]")]
    public class GateInwardController : ApiControllerBase
    {
        public GateInwardController(ISender mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllGateInwardAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllGateInwardQuery
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
        public async Task<IActionResult> GetGateInwardByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetGateInwardByIdQuery { Id = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetGateInwardAutoCompleteAsync([FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetGateInwardAutoCompleteQuery(term ?? string.Empty));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("pending-reference-docs")]
        public async Task<IActionResult> GetPendingReferenceDocsAsync(
            [FromQuery] int partyId,
            [FromQuery] int referenceDocumentTypeId)
        {
            var result = await Mediator.Send(new GetPendingReferenceDocsQuery
            {
                PartyId = partyId,
                ReferenceDocumentTypeId = referenceDocumentTypeId
            });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result.IsSuccess,
                message = result.Message,
                data = result.Data
            });
        }

        [HttpGet("pending-reference-doc-items")]
        public async Task<IActionResult> GetPendingReferenceDocItemsAsync(
            [FromQuery] int partyId,
            [FromQuery] int referenceDocumentTypeId,
            [FromQuery(Name = "poIds")] List<int>? poIds)
        {
            var result = await Mediator.Send(new GetPendingReferenceDocItemsQuery
            {
                PartyId = partyId,
                ReferenceDocumentTypeId = referenceDocumentTypeId,
                PoIds = poIds ?? []
            });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result.IsSuccess,
                message = result.Message,
                data = result.Data
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateGateInward([FromBody] CreateGateInwardCommand command)
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

        [HttpPost("upload-attachment")]
        public async Task<IActionResult> UploadAttachment([FromForm] UploadGateInwardAttachmentCommand command)
        {
            var result = await Mediator.Send(command);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "Attachment staged successfully.",
                data = result
            });
        }

        [HttpDelete("attachment")]
        public async Task<IActionResult> DeleteAttachment([FromQuery] int gateInwardHdrId)
        {
            var result = await Mediator.Send(new DeleteGateInwardAttachmentCommand(gateInwardHdrId));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Attachment deleted successfully." : "Attachment not found."
            });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteGateInward(int id)
        {
            var result = await Mediator.Send(new DeleteGateInwardCommand(id));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Gate Inward Entry deleted successfully." : "Failed to delete Gate Inward Entry."
            });
        }
    }
}
