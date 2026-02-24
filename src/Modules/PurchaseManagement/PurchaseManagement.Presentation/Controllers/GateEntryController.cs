using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Application.GRN.GateEntry.Commands.CreateGateEntry;
using PurchaseManagement.Application.GRN.GateEntry.Commands.DeleteGateEntryDocument;
using PurchaseManagement.Application.GRN.GateEntry.Commands.UploadGateEntryDocument;
using PurchaseManagement.Application.GRN.GateEntry.Queries.GetGateEntriesApprovedPo;
using PurchaseManagement.Application.GRN.GRNEntry.Queries.GetGateEntryPendingPo;
using MassTransit.Futures.Contracts;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace PurchaseManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class GateEntryController : ApiControllerBase
    {
        private readonly IMediator _mediator;

        public GateEntryController(IMediator mediator) : base(mediator) => _mediator = mediator;

        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateGateEntryCommand createGateEntryCommand)
        {

            // Process the command
            var CreatePartmasterId = await _mediator.Send(createGateEntryCommand);

            return Ok(new
            {
                StatusCode = StatusCodes.Status201Created,
                message = "Created successfully.",
                data = CreatePartmasterId
            });

        }

        [HttpPost("upload-document")]
        public async Task<IActionResult> UploadDocument(UploadGateEntryDocumentCommand uploadGateEntryDocumentCommand)
        {

            var file = await _mediator.Send(uploadGateEntryDocumentCommand);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "File uploaded successfully.",
                data = file,
                errors = ""
            });
        }

        [HttpDelete("delete-document")]
        public async Task<IActionResult> DeleteDocument([FromBody] DeleteGateEntryDocumentCommand deleteFileCommand)
        {
            if (deleteFileCommand == null || string.IsNullOrWhiteSpace(deleteFileCommand.GateEntrydocumentPath))
            {
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = "Invalid request. 'GateEntrydocumentPath' cannot be null or empty.",
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

          [HttpGet("{partyId}")]
        public async Task<IActionResult> GetPendingPoList(int partyId)
        {
            var partyMaster = await _mediator.Send(new GetGateEntriesApprovedPoQuery() { PartyId = partyId });

            if (partyMaster == null)
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    data = (object?)null,
                    message = $"PurchaseOrder with ID {partyId} not found"
                });
            }

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = partyMaster,
                message = "ID fetched successfully"
            });
        }


      
    }
}