using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.WorkCenter.Command.CreateWorkCenter;
using MaintenanceManagement.Application.WorkCenter.Command.DeleteWorkCenter;
using MaintenanceManagement.Application.WorkCenter.Command.UpdateWorkCenter;
using MaintenanceManagement.Application.WorkCenter.Queries.GetWorkCenter;
using MaintenanceManagement.Application.WorkCenter.Queries.GetWorkCenterAutoComplete;
using MaintenanceManagement.Application.WorkCenter.Queries.GetWorkCenterById;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MaintenanceManagement.Presentation.Controllers
{
    [Route("api/[controller]")]

    public class WorkCenterController : ApiControllerBase
    {
        private readonly ILogger<CostCenterController> _logger;
        private readonly IMediator _mediator;


        
        public WorkCenterController(ILogger<CostCenterController> logger,IMediator mediator)
        : base(mediator)
        {
            _logger = logger;
            _mediator=mediator;
        }

         [HttpGet]
        public async Task<IActionResult> GetAllWorkcenterAsync([FromQuery] int PageNumber,[FromQuery] int PageSize,[FromQuery] string? SearchTerm = null)
        {
           var workcenter = await Mediator.Send(
            new GetWorkCenterQuery
            {
                PageNumber = PageNumber, 
                PageSize = PageSize, 
                SearchTerm = SearchTerm
            });
            return Ok( new 
            { 
                StatusCode=StatusCodes.Status200OK, 
                data = workcenter.Data,
                TotalCount = workcenter.TotalCount,
                PageNumber = workcenter.PageNumber,
                PageSize = workcenter.PageSize
                });
        }
        [HttpGet("by-name")]
        public async Task<IActionResult> GetWorkcenter([FromQuery] string? WorkCenterName)
        {
        var workcenter = await Mediator.Send(new GetWorkCenterAutoCompleteQuery 
        { 
                SearchPattern = WorkCenterName ?? string.Empty 
        });

        return Ok(new { StatusCode = StatusCodes.Status200OK, data = workcenter.Data});
        }

        [HttpGet("{id}")]
        [ActionName(nameof(GetByIdAsync))]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var workcenter = await Mediator.Send(new GetWorkCenterByIdQuery() { Id = id});
          
            if(workcenter.IsSuccess)
            {
                
              return Ok(new { StatusCode=StatusCodes.Status200OK, data = workcenter.Data,message = workcenter.Message });
            }
            return NotFound( new { StatusCode=StatusCodes.Status404NotFound, message = workcenter.Message });   
        }
        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateWorkCenterCommand createWorkCenterCommand)
        {

            // Process the command
            var CreatedWorkCenterId = await _mediator.Send(createWorkCenterCommand);

            if (CreatedWorkCenterId.IsSuccess)
            {
            _logger.LogInformation($"WorkCenter {createWorkCenterCommand.WorkCenterCode} created successfully.");
            return Ok(new
            {
                StatusCode = StatusCodes.Status201Created,
                message =CreatedWorkCenterId.Message,
                data = CreatedWorkCenterId.Data
            });
            }
            _logger.LogWarning($"WorkCenter {createWorkCenterCommand.WorkCenterCode} Creation failed.");
            return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = CreatedWorkCenterId.Message
                });
        
        }
            [HttpPut]
            public async Task<IActionResult> UpdateAsync(UpdateWorkCenterCommand updateWorkCenterCommand)
            {
    

                    var updatedworkcenter = await _mediator.Send(updateWorkCenterCommand);

                    if (updatedworkcenter.IsSuccess)
                    {
                        _logger.LogInformation($"WorkCenter {updateWorkCenterCommand.WorkCenterName} updated successfully.");
                    return Ok(new
                        {
                            message = updatedworkcenter.Message,
                            statusCode = StatusCodes.Status200OK
                        });
                    }
                    _logger.LogWarning($"WorkCenter {updateWorkCenterCommand.WorkCenterName} Update failed.");
                    return NotFound(new
                    {
                        message =updatedworkcenter.Message,
                        statusCode = StatusCodes.Status404NotFound
                    });   
            }
            [HttpDelete]
            public async Task<IActionResult> DeleteWorkCenterAsync(int id)
            {

                    // Process the delete command
                    var result = await _mediator.Send(new DeleteWorkCenterCommand { Id = id });

                    if (result.IsSuccess) 
                    {
                        _logger.LogInformation($"WorkCenter {id} deleted successfully.");
                        return Ok(new
                        {
                            message = result.Message,
                            statusCode = StatusCodes.Status200OK
                        });
                        
                    }
                    _logger.LogWarning($"WorkCenter {id} Not Found or Invalid WorkCenterId.");
                    return NotFound(new
                    {
                        message = result.Message,
                        statusCode = StatusCodes.Status404NotFound
                    });
            
            }
      
    }
}