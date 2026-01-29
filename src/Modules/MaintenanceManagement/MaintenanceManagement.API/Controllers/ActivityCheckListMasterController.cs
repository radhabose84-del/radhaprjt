using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.ActivityCheckListMaster.Command.CreateActivityCheckListMaster;
using MaintenanceManagement.Application.ActivityCheckListMaster.Command.DeleteActivityCheckListMaster;
using MaintenanceManagement.Application.ActivityCheckListMaster.Command.UpdateActivityCheckListMaster;
using MaintenanceManagement.Application.ActivityCheckListMaster.Queries.GetActivityCheckListMaster;
using MaintenanceManagement.Application.ActivityCheckListMaster.Queries.GetActivityCheckListMasterById;
using MaintenanceManagement.Application.ActivityCheckListMaster.Queries.GetCheckListByActivityId;
using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.Common.Interfaces.IActivityMaster;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MaintenanceManagement.API.Controllers
{
    [Route("api/[controller]")]
    public class ActivityCheckListMasterController : ApiControllerBase
    {

        public ActivityCheckListMasterController(ISender mediator) : base(mediator)
        {
            

        }


        [HttpGet]
        public async Task<IActionResult> GetAllActivityCheckListMasterAsync([FromQuery] int PageNumber, [FromQuery] int PageSize, [FromQuery] string? SearchTerm = null)
        {
            var CheckListMaster = await Mediator.Send(
            new GetAllActivityCheckListMasterQuery
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                SearchTerm = SearchTerm
            });
            // var activecompanies = companies.Data.ToList(); 

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = CheckListMaster.Data,
                TotalCount = CheckListMaster.TotalCount,
                PageNumber = CheckListMaster.PageNumber,
                PageSize = CheckListMaster.PageSize
            });
        }

        [HttpGet("{id}")]
        [ActionName(nameof(GetByIdAsync))]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var CheckListMaster = await Mediator.Send(new GetActivityCheckListMasterByIdQuery() { Id = id });

            return Ok(new { StatusCode = StatusCodes.Status200OK, data = CheckListMaster, message = "" });
            
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateActivityCheckListMasterCommand command)
        {

            var response = await Mediator.Send(command);
            
                return Ok(new
                {
                    StatusCode = StatusCodes.Status201Created,
                    message = "Created successfully.",
                    errors = "",
                    data = response
                });
           

        }

        [HttpPut]
        public async Task<IActionResult> Update(UpdateActivityCheckListMasterCommand command)
        {

             await Mediator.Send(command);
           
                return Ok(new
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Updated successfully.",
                    Errors = ""
                });
            
           
        }
       

        [HttpPost("ByActivityId")]
        public async Task<IActionResult> GetCheckListByActivityIdAsync([FromBody] GetActivityCheckListByActivityIdQuery command)
        {
           

            var activityCheckList = await Mediator.Send(command);

           
                return Ok(new { StatusCode = StatusCodes.Status200OK, data = activityCheckList, message = "" });
            

            
        }
         [HttpDelete]
            public async Task<IActionResult> DeleteCostCenterAsync(int id)
            {

                    // Process the delete command
                     await Mediator.Send(new DeleteActivityCheckListMasterCommand { Id = id });

                    return Ok(new 
                    {
                        StatusCode = StatusCodes.Status200OK,
                        message = "Delete successfully."                
                    });
                
            
            }

    }
}
