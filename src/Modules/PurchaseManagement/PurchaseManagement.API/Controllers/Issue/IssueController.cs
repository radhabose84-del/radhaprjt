using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Application.Issue.Command.CreateIssueEntry;
using PurchaseManagement.Application.Issue.Queries.GetApprovedMrsById;
using PurchaseManagement.Application.Issue.Queries.GetPendingIssue;
using PurchaseManagement.Application.Issue.Queries.GetPendingIssueHeader;
using MassTransit.Futures.Contracts;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace PurchaseManagement.API.Controllers.Issue
{

    [ApiController]
    [Route("api/[controller]")]
    public class IssueController : ApiControllerBase
    {
        private readonly IMediator _mediator;

        public IssueController(IMediator mediator)
        : base(mediator)
        {
            _mediator = mediator;

        }
        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateIssueEntryCommand createIssueEntryCommand)
        {
            var CreateIssueEntry = await _mediator.Send(createIssueEntryCommand);

            return Ok(new
            {
                StatusCode = StatusCodes.Status201Created,
                message = "Created successfully.",
                data = CreateIssueEntry
            });
        }

        [HttpGet("{mrsId}")]
        [ActionName(nameof(GetPendingIssuesDetails))]
        public async Task<IActionResult> GetPendingIssuesDetails(int mrsId, CancellationToken cancellationToken)
        {
            var query = new GetPendingIssueQuery { MrsNo = mrsId };

            var result = await _mediator.Send(query, cancellationToken);

            if (result == null || result.Count == 0)
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "No pending issues found",
                    Data = (object?)null,
                });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                Data = result,
                Message = "Pending issues fetched successfully"
            });
        }

        [HttpGet("IssueEntryPendingHeaders")]
        public async Task<IActionResult> GetPendingIssueHeaderAsync(
           int PageNumber, [FromQuery] int PageSize, [FromQuery] string? SearchTerm = null, [FromQuery] DateTimeOffset? fromDate = null, [FromQuery] DateTimeOffset? toDate = null)
        {
            // Send query to repository or mediator
            var mrsPendingHeaders = await _mediator.Send(new GetPendingIssueHeaderQuery
            {

                PageNumber = PageNumber,
                PageSize = PageSize,
                SearchTerm = SearchTerm,
                FromDate = fromDate,
                ToDate = toDate,

            });

            if (mrsPendingHeaders == null)
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
                data = mrsPendingHeaders.Data,
                TotalCount = mrsPendingHeaders.TotalCount,
                PageNumber = mrsPendingHeaders.PageNumber,
                PageSize = mrsPendingHeaders.PageSize
            });
        }
        
        
        [HttpGet("by-name")]
        public async Task<IActionResult> GetMrsApprovedDetails([FromQuery] string? mrsNo)
        {
        var mrsPendingHeaders = await Mediator.Send(new GetApprovedMrsByIdQuery 
        { 
                SearchPattern = mrsNo ?? string.Empty 
        });

        return Ok(new { StatusCode = StatusCodes.Status200OK, data = mrsPendingHeaders});
        }


       
    }
}