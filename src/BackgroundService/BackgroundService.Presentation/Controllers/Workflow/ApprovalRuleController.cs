using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Presentation.Controllers.Notification;
using BackgroundService.Application.Workflow.ApprovalRules.Commands.CreateApprovalRule;
using BackgroundService.Application.Workflow.ApprovalRules.Commands.DeleteApprovalRule;
using BackgroundService.Application.Workflow.ApprovalRules.Commands.UpdateApprovalRule;
using BackgroundService.Application.Workflow.ApprovalRules.Queries.GetAllApprovalRule;
using BackgroundService.Application.Workflow.ApprovalRules.Queries.GetByIdApprovalRule;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BackgroundService.Presentation.Controllers.Workflow
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApprovalRuleController : ApiControllerBase
    {
        private readonly IMediator _mediator;
        public ApprovalRuleController(IMediator mediator)
        : base(mediator)
        {
            _mediator = mediator;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllApprovalRuleAsync([FromQuery] int PageNumber, [FromQuery] int PageSize, [FromQuery] string? SearchTerm = null)
        {
            var ApprovalRule = await Mediator.Send(
             new GetAllApprovalRuleQuery
             {
                 PageNumber = PageNumber,
                 PageSize = PageSize,
                 SearchTerm = SearchTerm
             });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = ApprovalRule.Data,
                TotalCount = ApprovalRule.TotalCount,
                PageNumber = ApprovalRule.PageNumber,
                PageSize = ApprovalRule.PageSize
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateApprovalRuleCommand createApprovalRuleCommand)
        {
            var CreatedApprovalStep = await _mediator.Send(createApprovalRuleCommand);
            return Ok(new
            {
                StatusCode = StatusCodes.Status201Created,
                message = "Created successfully.",
                data = CreatedApprovalStep
            });

        }
        [HttpPut]
        public async Task<IActionResult> UpdateAsync(UpdateApprovalRuleCommand updateApprovalRuleCommand)
        {
            await _mediator.Send(updateApprovalRuleCommand);
            return Ok(new
            {
                message = "Updated successfully.",
                statusCode = StatusCodes.Status200OK
            });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            await _mediator.Send(new DeleteApprovalRuleCommand { Id = id });
            return Ok(new
            {
                message = "Deleted successfully.",
                statusCode = StatusCodes.Status200OK
            });

        }
          [HttpGet("{id}")]        
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var ApprovalStep = await Mediator.Send(new GetByIdApprovalRuleQuery() { Id = id});           
            return Ok(new { StatusCode=StatusCodes.Status200OK, data = ApprovalStep,message = "" });            
        }
    }
}