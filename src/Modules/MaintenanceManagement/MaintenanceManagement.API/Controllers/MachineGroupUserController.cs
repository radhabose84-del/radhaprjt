
using MaintenanceManagement.Application.MachineGroupUser.Command.DeleteMachineGroupUser;
using MaintenanceManagement.Application.MachineGroupUser.Command.UpdateMachineGroupUser;
using MaintenanceManagement.Application.MachineGroupUser.Queries.GetMachineGroupUser;
using MaintenanceManagement.Application.MachineGroupUser.Queries.GetMachineGroupUserAutoComplete;
using MaintenanceManagement.Application.MachineGroupUser.Queries.GetMachineGroupUserById;
using MaintenanceManagement.Application.MachineGroupUsers.Command.CreateMachineGroupUser;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MaintenanceManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MachineGroupUserController  : ApiControllerBase
    {
        
        
        public MachineGroupUserController(ISender mediator)
        : base(mediator)
        {
            
        }
           [HttpGet]
        public async Task<IActionResult> GetAllShiftMastersAsync([FromQuery] int PageNumber,[FromQuery] int PageSize,[FromQuery] string? SearchTerm = null)
        {
           var divisions = await Mediator.Send(
            new GetMachineGroupUserQuery
            {
                PageNumber = PageNumber, 
                PageSize = PageSize, 
                SearchTerm = SearchTerm
            });
           
            return Ok( new 
            { 
                StatusCode=StatusCodes.Status200OK, 
                data = divisions.Data,
                TotalCount = divisions.TotalCount,
                PageNumber = divisions.PageNumber,
                PageSize = divisions.PageSize
                });
        }
         [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateMachineGroupUserCommand command)
        {
            
            
            var response = await Mediator.Send(command);
           
                return Ok(new 
                { 
                    StatusCode=StatusCodes.Status201Created, 
                    message = "Created Successfully", 
                    errors = "", 
                    data = response 
                });
            
        }
         [HttpGet("{id}")]
         [ActionName(nameof(GetByIdAsync))]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
               
            var shiftMaster = await Mediator.Send(new GetMachineGroupUserByIdQuery { Id = id});
          
            return Ok(new 
            { 
                StatusCode=StatusCodes.Status200OK, 
                data = shiftMaster
            });
        }

        [HttpPut]
        public async Task<IActionResult> Update( UpdateMachineGroupUserCommand command )
        {
         

              await Mediator.Send(command);
             
                 return Ok(new 
                 { 
                    StatusCode=StatusCodes.Status200OK, 
                    message = "Updated Successfully", 
                    errors = "" 
                });
             
            
           
        }


        [HttpDelete("{id}")]        
        public async Task<IActionResult> Delete(int id)
        {
            var command = new DeleteMachineGroupUserCommand { Id = id };
          
            await Mediator.Send(command);

            return Ok(new 
            { 
                StatusCode=StatusCodes.Status200OK, 
                message = "Deleted Successfully", 
                errors = "" 
            });
            
        }

         [HttpGet("by-name")]
        public async Task<IActionResult> GetShiftMaster([FromQuery] string? name)
        {
           
           var shiftMaster = await Mediator.Send(new GetMachineGroupUserAutoCompleteQuery {SearchPattern = name});
            return Ok( new 
            { 
                StatusCode=StatusCodes.Status200OK, 
                data = shiftMaster 
            });
        }
    }
}