using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.ActivityCheckListMaster.Command.UpdateActivityCheckListMaster;
using MaintenanceManagement.Application.ActivityMaster.Command.CreateActivityMaster;
using MaintenanceManagement.Application.ActivityMaster.Command.UpdateActivityMster;
using MaintenanceManagement.Application.ActivityMaster.Queries.GetActivityByMachinGroupId;
using MaintenanceManagement.Application.ActivityMaster.Queries.GetActivityType;
using MaintenanceManagement.Application.ActivityMaster.Queries.GetAllActivityMaster;
using MaintenanceManagement.Application.ActivityMaster.Queries.GetMachineGroupById;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IActivityMaster;
using MaintenanceManagement.Application.MachineGroup.Queries.GetActivityMasterAutoComplete;
using MaintenanceManagement.Application.MachineGroup.Queries.GetMachineGroupById;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MaintenanceManagement.Presentation.Controllers
{
     [Route("api/[controller]")]
    public class ActivityMasterController : ApiControllerBase
    {
        
        public ActivityMasterController(ISender mediator ):base(mediator)
        {
            
          
        }

            [HttpGet] 
          public async Task<IActionResult> GetAllActivityMasterAsync([FromQuery] int PageNumber,[FromQuery] int PageSize,[FromQuery] string? SearchTerm = null)
        {
            var activitymaster = await Mediator.Send(
            new GetAllActivityMasterQuery
            {
                PageNumber = PageNumber, 
                PageSize = PageSize, 
                SearchTerm = SearchTerm
            });
           // var activecompanies = companies.Data.ToList(); 

            return Ok(new 
            { 
                StatusCode=StatusCodes.Status200OK, 
                data = activitymaster.Data,
                TotalCount = activitymaster.TotalCount,
                PageNumber = activitymaster.PageNumber,
                PageSize = activitymaster.PageSize
            });
        }



         [HttpGet("{id}")]
        [ActionName(nameof(GetByIdAsync))]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
           
            var activitymaster = await Mediator.Send(new GetActivityMasterByIdQuery() { Id = id});
          
           
                 return Ok(new { StatusCode=StatusCodes.Status200OK, data = activitymaster,message="" });
            
           
        }
         [HttpGet("MachineGroup/{activityId}")]
         [ActionName(nameof(GetMachineGroupById))]
         public async Task<IActionResult> GetMachineGroupById(int activityId)
         {
             var MachineGroup = await Mediator.Send(new GetMachineGroupNameByIdQuery() { ActivityId = activityId});
          
            
                 return Ok(new { StatusCode=StatusCodes.Status200OK, data = MachineGroup,message="" });


         }

         [HttpGet("by-name")]
        public async Task<IActionResult> GetMachineGroup([FromQuery] string? name , [FromQuery] string? machineCode)
        {
          
            var activitymaster = await Mediator.Send(new GetActivityMasterAutoCompleteQuery {SearchPattern = name , MachineCode = machineCode});
          
            return Ok( new { StatusCode=StatusCodes.Status200OK, data = activitymaster });
        
        }
        
         [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateActivityMasterCommand command)
        {
          
            var response = await Mediator.Send(command);
                                       
                return Ok(new 
                {
                     StatusCode=StatusCodes.Status201Created,
                 message = "Created successfully.",
                  errors = "",
                  data = response 
                  });
                        
        } 

        [HttpPut("update")]
        public async Task<IActionResult> UpdateActivityMaster([FromBody] UpdateActivityMasterCommand command)
        {

            var result = await Mediator.Send(command);


                return Ok(new
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Updated successfully.",
                    errors = "" 
                });
          }


              [HttpGet("GetActivityType")]
                public async Task<IActionResult> GetActivityTypeAsync()
                {
                    var result = await Mediator.Send(new GetActivityTypeQuery());
                    if (result == null || result.Data == null || result.Data.Count == 0)
                    {
                        return NotFound(new
                        {
                            StatusCode = StatusCodes.Status404NotFound,
                            message = "No ActivityType  found."
                        });
                    }
                    return Ok(new
                    {
                        StatusCode = StatusCodes.Status200OK,
                        message = "ActivityType  fetched successfully.",
                        data = result.Data
                    });
                }


        [HttpGet("GetActivity/{machineGroupId}")]
        public async Task<IActionResult> GetActivityByMachineGroupId(int machineGroupId)
        {
            var response = await Mediator.Send(new GetActivityByMachinGroupIdQuery
            {
                MachineGroupId = machineGroupId
            });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                Message = response,
                Data = response
            });
        }




    }

}