using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.API.Controller.Notification;
using Contracts.Common;
using BackgroundService.Application.Workflow.ApprovalRequests.Commands.ApproveApprovalRequest;
using BackgroundService.Application.Workflow.ApprovalRequests.Commands.ApproveDocumentUpload;
using BackgroundService.Application.Workflow.ApprovalRequests.Queries.ApprovalDocumentDownload;
using BackgroundService.Application.Workflow.ApprovalRequests.Queries.GetApprovedHistory;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BackgroundService.API.Controller.Workflow
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApprovalRequestController : ApiControllerBase
    {
        private readonly IMediator _mediator;
        public ApprovalRequestController(IMediator mediator)
        : base(mediator)
        {
            _mediator = mediator;
        }

        /*         [HttpPost("approve")]
                public async Task<IActionResult> ApproveAsync(ApproveApprovalRequestCommand approveApprovalRequestCommand)
                {
                    var ApproveReq = await _mediator.Send(approveApprovalRequestCommand);
                    return Ok(new
                    {
                        StatusCode = StatusCodes.Status201Created, 
                        message = "Approved successfully.",
                        data = ApproveReq
                    });

                }
         */
        [HttpPost("approve")]
        public async Task<IActionResult> ApproveBatchAsync([FromBody] ApproveApprovalRequestBatchCommand command)
        {
            var result = await _mediator.Send(command);

            var message =
                result.FailedCount > 0
                    ? $"Approved completed with errors. Approved={result.ApprovedCount}, Rejected={result.RejectedCount}, Failed={result.FailedCount}"
                : result.RejectedCount > 0 && result.ApprovedCount == 0
                    ? $"{result.RejectedCount} Items Rejected successfully."
                : result.ApprovedCount > 0 && result.RejectedCount == 0
                    ? $" {result.ApprovedCount} Items Approved successfully."
                : $"Partially {result.ApprovedCount} Items Approved  and {result.RejectedCount} Items Rejected successfully.";

            return Ok(new
            {
                statusCode = StatusCodes.Status200OK,
                message,
                data = result
            });
        }
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(UploadFileCommand uploadFileCommand)
        {
            var Result = await _mediator.Send(uploadFileCommand);
            return Ok(new
            {
                StatusCode = StatusCodes.Status201Created,
                message = "Upload successfully.",
                data = Result
            });

        }
        [HttpGet("download")]
        public async Task<IActionResult> DownloadFile([FromQuery] string relativePath)
        {
            var result = await Mediator.Send(new DownloadFileQuery { RelativePath = relativePath });

            return File(result.FileBytes, result.ContentType, result.FileName);
        }
        [HttpGet("approved-history")]
        public async Task<IActionResult> GetApprovedHistoryAsync([FromQuery] GetApprovedHistoryQuery approvedHistoryQuery)
        {
            var ApprovalRequest = await Mediator.Send(approvedHistoryQuery);
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = ApprovalRequest
            });
        }
        

         [HttpGet("by-module")]
               public async Task<ActionResult<List<Application.Workflow.ApprovalRequests.Queries.GetApprovalRequestById.ApprovalRequestWithLinesDto>>> GetByModule(
            [FromQuery] int moduleTransactionId,
            [FromQuery] int workflowTypeId)
        {
            var query = new Application.Workflow.ApprovalRequests.Queries.GetApprovalRequestById.GetApprovalRequestByModuleQuery
            {
                ModuleTransactionId = moduleTransactionId,
                WorkflowTypeId      = workflowTypeId
            };

            var result = await _mediator.Send(query);

            return Ok(new  
            
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }
    }
}