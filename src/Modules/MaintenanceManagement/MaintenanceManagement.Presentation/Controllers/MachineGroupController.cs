using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.Interfaces.IMachineGroup;
using MaintenanceManagement.Application.MachineGroup.Command.CreateMachineGroup;
using MaintenanceManagement.Application.MachineGroup.Command.DeleteMachineGroup;
using MaintenanceManagement.Application.MachineGroup.Command.UpdateMachineGroup;
using MaintenanceManagement.Application.MachineGroup.Queries.GetMachineGroup;
using MaintenanceManagement.Application.MachineGroup.Queries.GetMachineGroupAutoComplete;
using MaintenanceManagement.Application.MachineGroup.Queries.GetMachineGroupById;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MaintenanceManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class MachineGroupController : ApiControllerBase
    {
          

        

        public MachineGroupController(ISender mediator   ):base(mediator)
        {
            
          
        }

        [HttpGet] 
          public async Task<IActionResult> GetAllMachineGroupsAsync([FromQuery] int PageNumber,[FromQuery] int PageSize,[FromQuery] string? SearchTerm = null)
        {
            var machinegroup = await Mediator.Send(
            new GetMachineGroupQuery
            {
                PageNumber = PageNumber, 
                PageSize = PageSize, 
                SearchTerm = SearchTerm
            });
           // var activecompanies = companies.Data.ToList(); 

            return Ok(new 
            { 
                StatusCode=StatusCodes.Status200OK, 
                data = machinegroup.Data,
                TotalCount = machinegroup.TotalCount,
                PageNumber = machinegroup.PageNumber,
                PageSize = machinegroup.PageSize
            });
        }

        [HttpGet("{id}")]
        [ActionName(nameof(GetByIdAsync))]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
           
            var machinegroup = await Mediator.Send(new GetMachineGroupByIdQuery() { Id = id});
          
            return Ok(new { StatusCode=StatusCodes.Status200OK, data = machinegroup,message=machinegroup});
           
        }


         [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateMachineGroupCommand command)
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

         [HttpPut]        
        public async Task<IActionResult> UpdateAsync(UpdateMachineGroupCommand command)
        {         
          
            var result = await Mediator.Send(command);
           
                return Ok(new 
                {   StatusCode=StatusCodes.Status200OK,
                    message = "Updated successfully.", 
                    asset = result
                });
            
                
        }

       [ HttpDelete("{id}")]
          public async Task<IActionResult> Delete(int id)
        {
           
            await Mediator.Send(new DeleteMachineGroupCommand { Id = id });

            return Ok(new { StatusCode=StatusCodes.Status200OK, message = "Deleted successfully.", errors = "" });
              
            
        }
         [HttpGet("by-name")]
        public async Task<IActionResult> GetMachineGroup([FromQuery] string? name)
        {
          
            var miscmaster = await Mediator.Send(new GetMachineGroupAutoCompleteQuery {SearchPattern = name});
          
            return Ok( new { StatusCode=StatusCodes.Status200OK, data = miscmaster });
            
        }
        



    }
}