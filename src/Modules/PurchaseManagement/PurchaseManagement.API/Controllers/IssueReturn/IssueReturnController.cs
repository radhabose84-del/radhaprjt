using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Application.IssueReturn.Command.CreateIssueReturn;
using PurchaseManagement.Application.IssueReturn.Command.UpdateIssueReturn;
using PurchaseManagement.Application.IssueReturn.Queries.GetIssueDetailsById;
using PurchaseManagement.Application.IssueReturn.Queries.GetPendingIssueReturn;
using PurchaseManagement.Application.IssueReturn.Queries.GetPendingIssueReturnById;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace PurchaseManagement.API.Controllers.IssueReturn
{
    [ApiController]
    [Route("api/[controller]")]
    public class IssueReturnController : ApiControllerBase
    {
        private readonly IMediator _mediator;

        public IssueReturnController(IMediator mediator)
        : base(mediator)
        {
            _mediator = mediator;

        }
        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateIssueReturnEntryCommand createIssueReturnEntry)
        {
            var CreateIssueEntry = await _mediator.Send(createIssueReturnEntry);

            return Ok(new
            {
                StatusCode = StatusCodes.Status201Created,
                message = "Created successfully.",
                data = CreateIssueEntry
            });
        }
        [HttpGet("{issueHeaderId}")]
        [ActionName(nameof(GetIssueDetailsById))]
        public async Task<IActionResult> GetIssueDetailsById(int issueHeaderId, [FromQuery] int? itemId, CancellationToken cancellationToken)
        {
            var query = new GetIssueDetailsByIdQuery
            {
                IssueHeaderId = issueHeaderId,
                ItemId = itemId
            };

            var result = await _mediator.Send(query, cancellationToken);

            if (result == null || result.Count == 0)
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "No issue details found for the specified IssueHeaderId.",
                    Data = (object?)null
                });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                Data = result,
                Message = "Issue details fetched successfully."
            });
        }
        [HttpPut]
        public async Task<IActionResult> UpdateAsync(UpdateIssueReturnEntryCommand updateIssueReturnEntryCommand)
        {

            await _mediator.Send(updateIssueReturnEntryCommand);

            return Ok(new
            {
                message = "Updated successfully.",
                statusCode = StatusCodes.Status200OK
            });

        }
        
         [HttpGet("pending")]
        public async Task<IActionResult> GetPendingIssueReturnAsync([FromQuery] int PageNumber, [FromQuery] int PageSize, [FromQuery] string? SearchTerm = null)
        {
            var IssueReturn = await Mediator.Send(
             new GetPendingIssueReturnQuery
             {
                 PageNumber = PageNumber,
                 PageSize = PageSize,
                 SearchTerm = SearchTerm
             });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = IssueReturn.Data,
                TotalCount = IssueReturn.TotalCount,
                PageNumber = IssueReturn.PageNumber,
                PageSize = IssueReturn.PageSize
            });
        }

        [HttpGet("pending/{id}")]
        public async Task<IActionResult> GetPendingIssueReturnByIdAsync(int id)
        {
            var Indent = await Mediator.Send(new GetPendingIssueReturnByIdQuery() { Id = id });
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = Indent, message = "" });
        }




       
    }
}