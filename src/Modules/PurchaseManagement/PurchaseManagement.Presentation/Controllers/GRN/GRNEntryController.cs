using PurchaseManagement.Application.GRN.GRNEntry.Commands;
using PurchaseManagement.Application.GRN.GRNEntry.Commands.CreateGRNPutaway;
using PurchaseManagement.Application.GRN.GRNEntry.Commands.DeleteGRNDocument;
using PurchaseManagement.Application.GRN.GRNEntry.Commands.UpdateGRNEntry;
using PurchaseManagement.Application.GRN.GRNEntry.Commands.UploadGRNDocument;
using PurchaseManagement.Application.GRN.GRNEntry.Queries.GetGateEntryPending;
using PurchaseManagement.Application.GRN.GRNEntry.Queries.GetGateEntryPendingPo;
using PurchaseManagement.Application.GRN.GRNEntry.Queries.GetGrnPending;
using PurchaseManagement.Application.GRN.GRNEntry.Queries.GetGrnPendingDetails;
using PurchaseManagement.Application.GRN.GRNEntry.Queries.GetGrnPendingHeader;
using PurchaseManagement.Application.GRN.GRNEntry.Queries.GetGrnQCCompletedDetails;
using PurchaseManagement.Application.GRN.GRNEntry.Queries.GetPoPending;
using MediatR; // ✅ correct namespace
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace PurchaseManagement.Presentation.Controllers.GRN
{
    [Route("api/[controller]")]
    public class GRNEntryController : ApiControllerBase
    {
        private readonly IMediator _mediator;

        public GRNEntryController(IMediator mediator)
        : base(mediator)
        {
            _mediator = mediator;



        }



        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateGRNEntryCommand createGRNEntryCommand)
        {
            var createGateEntry = await _mediator.Send(createGRNEntryCommand);

            return Ok(new
            {
                StatusCode = StatusCodes.Status201Created,
                message = "Created successfully.",
                data = createGateEntry
            });
        }

        [HttpPost("uploadGRN-document")]
        public async Task<IActionResult> UploadDocument(UploadGrnEntryDocumentCommand uploadGrnEntryDocumentCommand)
        {

            var file = await _mediator.Send(uploadGrnEntryDocumentCommand);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "File uploaded successfully.",
                data = file,
                errors = ""
            });
        }
        [HttpDelete("deleteGRN-document")]
        public async Task<IActionResult> DeleteDocument([FromBody] DeleteGRNEntryDocumentCommand deleteFileCommand)
        {
            if (deleteFileCommand == null || string.IsNullOrWhiteSpace(deleteFileCommand.GrnEntrydocumentPath))
            {
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = "Invalid request. 'GRNEntrydocumentPath' cannot be null or empty.",
                    errors = ""
                });
            }
            var file = await _mediator.Send(deleteFileCommand);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "File deleted successfully.",
                errors = ""
            });
        }

        [HttpPost("uploadGRNDetail-document")]
        public async Task<IActionResult> UploadGRNDetailDocument(UploadGrnDetailDocumentCommand uploadGrnDetailDocumentCommand)
        {

            var file = await _mediator.Send(uploadGrnDetailDocumentCommand);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "File uploaded successfully.",
                data = file,
                errors = ""
            });
        }
        [HttpDelete("deleteGRNDetail-document")]
        public async Task<IActionResult> DeleteGRNDetailDocument([FromBody] DeleteGrnDetailDocumentCommand deleteGrnDetailDocumentCommand)
        {
            if (deleteGrnDetailDocumentCommand == null || string.IsNullOrWhiteSpace(deleteGrnDetailDocumentCommand.GrnDetaildocumentPath))
            {
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = "Invalid request. 'GrnDetaildocumentPath' cannot be null or empty.",
                    errors = ""
                });
            }
            var file = await _mediator.Send(deleteGrnDetailDocumentCommand);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "File deleted successfully.",
                errors = ""
            });
        }

        [HttpPost("uploadGRNQC-document")]
        public async Task<IActionResult> UploadGRNQCDocument(UploadGrnQcDocumentCommand uploadGrnQcDocumentCommand)
        {

            var file = await _mediator.Send(uploadGrnQcDocumentCommand);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "File uploaded successfully.",
                data = file,
                errors = ""
            });
        }
        [HttpDelete("deleteGRNQC-document")]
        public async Task<IActionResult> DeleteGRNQCDocument([FromBody] DeleteGRNQcDocumentCommand deleteGRNQcDocumentCommand)
        {
            if (deleteGRNQcDocumentCommand == null || string.IsNullOrWhiteSpace(deleteGRNQcDocumentCommand.GrnQcdocumentPath))
            {
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = "Invalid request. 'GRNQcdocumentPath' cannot be null or empty.",
                    errors = ""
                });
            }
            var file = await _mediator.Send(deleteGRNQcDocumentCommand);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "File deleted successfully.",
                errors = ""
            });
        }
        [HttpPut]
        public async Task<IActionResult> UpdateAsync(UpdateGRNEntryCommand updateGRNEntryCommand)
        {

            await _mediator.Send(updateGRNEntryCommand);

            return Ok(new
            {
                message = "Updated successfully.",
                statusCode = StatusCodes.Status200OK
            });

        }

        [HttpGet("{partyId}")]
        public async Task<IActionResult> GetPendingPoList(int partyId)
        {
            var partyMaster = await _mediator.Send(new GetGateEntryPendingPoQuery() { PartyId = partyId });

            if (partyMaster == null)
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    data = (object?)null,
                    message = $"PartyMaster with ID {partyId} not found"
                });
            }

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = partyMaster,
                message = "ID fetched successfully"
            });
        }

        [HttpGet("GateEntryPendingPo/{partyId}/{poId}")]
        public async Task<IActionResult> GetPendingPoGateList(int partyId, int poId)
        {
            var partyMaster = await _mediator.Send(new GetGateEntryPendingQuery() { PartyId = partyId, PoId = poId });

            if (partyMaster == null)
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    data = (object?)null,
                    message = $"PartyMaster with ID {poId} not found"
                });
            }

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = partyMaster,
                message = "ID fetched successfully"
            });
        }

        [HttpGet("GetPendingPoGrnList/{partyId}/{poId}/{gateEntryId}")]
        public async Task<IActionResult> GetPendingPoGrnList(int partyId, int poId, int gateEntryId)
        {
            var partyMaster = await _mediator.Send(new GetGrnPendingQuery() { PartyId = partyId, PoId = poId, GateEntryId = gateEntryId });

            if (partyMaster == null)
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    data = (object?)null,
                    message = $"PartyMaster with ID {gateEntryId} not found"
                });
            }

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = partyMaster,
                message = "ID fetched successfully"
            });
        }

        [HttpGet("GRNEntryPendingPo")]
        public async Task<IActionResult> GRNEntryPendingPo(
             [FromQuery] int? GrnId,
             [FromQuery] bool? IsGrnGenerated,
             [FromQuery] bool? IsQcGenerated
             )
        {
            // Send query to mediator
            var grnPendingDetails = await _mediator.Send(new GetGrnPendingDetailsQuery
            {
                GrnId = GrnId,
                IsGrnGenerated = IsGrnGenerated,
                IsQcGenerated = IsQcGenerated
                
            });

            if (grnPendingDetails == null || !grnPendingDetails.Any())
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    data = (object?)null,
                    message = GrnId.HasValue
                        ? $"GRN with ID {GrnId} not found"
                        : "No pending GRN entries found"
                });
            }

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = grnPendingDetails,
                message = "GRN pending details fetched successfully"
            });
        }

        [HttpGet("GRNEntryPendingHeader")]
        public async Task<IActionResult> GRNEntryPendingHeader(
            [FromQuery] DateTimeOffset? fromDate,
            [FromQuery] DateTimeOffset? toDate,
            [FromQuery] bool? IsGrnGenerated,
            [FromQuery] bool? IsQcGenerated,int PageNumber,[FromQuery] int PageSize,[FromQuery] string? SearchTerm = null)
        {
            // Send query to repository or mediator
            var grnPendingHeaders = await _mediator.Send(new GetGrnPendingHeaderQuery
            {
                FromDate = fromDate,
                ToDate = toDate,
                IsGrnGenerated = IsGrnGenerated,
                IsQcGenerated = IsQcGenerated,
                PageNumber = PageNumber, 
                PageSize = PageSize, 
                SearchTerm = SearchTerm
                
            });

            if (grnPendingHeaders == null)
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    data = (object?)null,
                    message = "No pending GRN headers found"
                });
            }

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = grnPendingHeaders.Data,
                TotalCount = grnPendingHeaders.TotalCount,
                PageNumber = grnPendingHeaders.PageNumber,
                PageSize = grnPendingHeaders.PageSize
            });
        }

        [HttpGet("GrnQcCompletedHeader")]
        public async Task<IActionResult> GrnQcCompletedHeader(
           [FromQuery] DateTimeOffset? fromDate,
           [FromQuery] DateTimeOffset? toDate,[FromQuery] int PageNumber,[FromQuery] int PageSize,[FromQuery] string? SearchTerm = null)
        {
            // Send query to repository or mediator
            var grnPendingHeaders = await _mediator.Send(new GetGrnQCCompletedDetailsHeaderQuery
            {
                FromDate = fromDate,
                ToDate = toDate,
                PageNumber = PageNumber, 
                PageSize = PageSize, 
                SearchTerm = SearchTerm

            });

            if (grnPendingHeaders == null)
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    data = (object?)null,
                    message = "No pending GRN headers found"
                });
            }

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = grnPendingHeaders.Data,
                TotalCount = grnPendingHeaders.TotalCount,
                PageNumber = grnPendingHeaders.PageNumber,
                PageSize = grnPendingHeaders.PageSize
            });
        }

        [HttpGet("GrnQcCompletedDetails/{grnid}")]
        public async Task<IActionResult> GetPendingPoGrnList(int grnid,[FromQuery] int itemId)
        {
            var partyMaster = await _mediator.Send(new GetGrnQCCompletedDetailsQuery() { GrnId = grnid,ItemId= itemId });

            if (partyMaster == null)
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    data = (object?)null,
                    message = $"Grn with ID {grnid} not found"
                });
            }

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = partyMaster,
                message = "ID fetched successfully"
            });
        }

        [HttpGet("po-pending")]
        public async Task<IActionResult> GetPoPendingAsync()
        {
            var result = await _mediator.Send(new GetPoPendingQuery());

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result,
                message = "Pending PO list fetched successfully"
            });
        }

        [HttpPost("CreateGrnPutAway")]
        public async Task<IActionResult> CreateGrnPutAway(CreateGRNPutawayCommand createGRNPutawayCommand)
        {
            var createGateEntry = await _mediator.Send(createGRNPutawayCommand);

            return Ok(new
            {
                StatusCode = StatusCodes.Status201Created,
                message = "Created successfully.",
                data = createGateEntry
            });
        }




    }
}
