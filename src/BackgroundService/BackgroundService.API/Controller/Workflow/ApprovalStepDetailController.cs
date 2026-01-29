using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.API.Controller.Notification;
using BackgroundService.Application.Workflow.ApprovalStepDetails.Commands.CreateApprovalStepDetail;
using BackgroundService.Application.Workflow.ApprovalStepDetails.Commands.DeleteApprovalStepDetail;
using BackgroundService.Application.Workflow.ApprovalStepDetails.Commands.UpdateApprovalStepDetail;
using BackgroundService.Application.Workflow.ApprovalStepDetails.Queries.GetAllApprovalStepDetail;
using BackgroundService.Application.Workflow.ApprovalStepDetails.Queries.GetApprovalStepDetailAutoComplete;
using BackgroundService.Application.Workflow.ApprovalStepDetails.Queries.GetApprovalStepDetailById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BackgroundService.API.Controller.Workflow
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApprovalStepDetailController : ApiControllerBase
    {
        private readonly IMediator _mediator;
        public ApprovalStepDetailController(IMediator mediator)
        : base(mediator)
        {
            _mediator = mediator;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllApprovalStepDetailAsync([FromQuery] int PageNumber, [FromQuery] int PageSize, [FromQuery] string? SearchTerm = null)
        {
            var ApprovalStep = await Mediator.Send(
             new GetAllApprovalStepDetailQuery
             {
                 PageNumber = PageNumber,
                 PageSize = PageSize,
                 SearchTerm = SearchTerm
             });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = ApprovalStep.Data,
                TotalCount = ApprovalStep.TotalCount,
                PageNumber = ApprovalStep.PageNumber,
                PageSize = ApprovalStep.PageSize
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateApprovalStepDetailCommand createApprovalStepDetailCommand)
        {
            var CreatedApprovalStep = await _mediator.Send(createApprovalStepDetailCommand);
            return Ok(new
            {
                StatusCode = StatusCodes.Status201Created,
                message = "Created successfully.",
                data = CreatedApprovalStep
            });

        }
        [HttpPut]
        public async Task<IActionResult> UpdateAsync(UpdateApprovalStepDetailCommand updateApprovalStepDetailCommand)
        {
            await _mediator.Send(updateApprovalStepDetailCommand);
            return Ok(new
            {
                message = "Updated successfully.",
                statusCode = StatusCodes.Status200OK
            });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            await _mediator.Send(new DeleteApprovalStepDetailCommand { Id = id });
            return Ok(new
            {
                message = "Deleted successfully.",
                statusCode = StatusCodes.Status200OK
            });

        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var ApprovalStep = await Mediator.Send(new GetApprovalStepDetailByIdQuery() { Id = id });
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = ApprovalStep, message = "" });
        }
         [HttpGet("by-name")]
        public async Task<IActionResult> GetApprovalStepAutoCompleteAsync([FromQuery] string? SearchPattern)
        {
            var ApprovalRule = await Mediator.Send(new GetApprovalStepDetailAutoCompleteQuery
            {
                SearchPattern = SearchPattern ?? string.Empty
            });
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = ApprovalRule });
        }
    }
}