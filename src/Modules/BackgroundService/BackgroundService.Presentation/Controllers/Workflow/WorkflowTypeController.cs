using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Presentation.Controllers.Notification;
using BackgroundService.Application.Workflow.WorkflowTypes.Commands.CreateWorkflowType;
using BackgroundService.Application.Workflow.WorkflowTypes.Commands.DeleteWorkflowType;
using BackgroundService.Application.Workflow.WorkflowTypes.Commands.UpdateWorkflowType;
using BackgroundService.Application.Workflow.WorkflowTypes.Queries.GetAllWorkflowType;
using BackgroundService.Application.Workflow.WorkflowTypes.Queries.GetWorkflowTypeAutoComplete;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BackgroundService.Presentation.Controllers.Workflow
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkflowTypeController : ApiControllerBase
    {
        private readonly IMediator _mediator;
        public WorkflowTypeController(IMediator mediator)
        : base(mediator)
        {
            _mediator = mediator;
        }
           [HttpGet]
        public async Task<IActionResult> GetAllWorkflowTypeAsync([FromQuery] int PageNumber,[FromQuery] int PageSize,[FromQuery] string? SearchTerm = null)
        {
           var WorkflowType = await Mediator.Send(
            new GetAllWorkflowTypeQuery
            {
                PageNumber = PageNumber, 
                PageSize = PageSize, 
                SearchTerm = SearchTerm
            });
            return Ok( new 
            { 
                StatusCode=StatusCodes.Status200OK, 
                data = WorkflowType.Data,
                TotalCount = WorkflowType.TotalCount,
                PageNumber = WorkflowType.PageNumber,
                PageSize = WorkflowType.PageSize
                });
        }
        
        [HttpGet("by-name")]
        public async Task<IActionResult> GetWorkflowTypeAutoCompleteAsync([FromQuery] string? ModuleName)
        {
            var WorkflowType = await Mediator.Send(new GetWorkflowTypeAutoCompleteQuery 
            { 
                    SearchPattern = ModuleName ?? string.Empty 
            });
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = WorkflowType});
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateWorkflowTypeCommand createWorkflowTypeCommand)
        {            
            var CreatedWorkflow = await _mediator.Send(createWorkflowTypeCommand);            
            return Ok(new
            {
                StatusCode = StatusCodes.Status201Created,
                message ="Created successfully.",
                data = CreatedWorkflow
            });            
        
        }
        [HttpPut]
        public async Task<IActionResult> UpdateAsync(UpdateWorkflowTypeCommand updateWorkflowTypeCommand)
        {
            await _mediator.Send(updateWorkflowTypeCommand);            
            return Ok(new
            {
                message = "Updated successfully.",
                statusCode = StatusCodes.Status200OK
            });                
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            await _mediator.Send(new DeleteWorkflowTypeCommand { Id = id });
            return Ok(new
            {
                message = "Deleted successfully.",
                statusCode = StatusCodes.Status200OK
            });
        
        }
    }
}