#nullable disable
using UserManagement.Application.UserGroup.Commands.CreateUserGroup;
using UserManagement.Application.UserGroup.Commands.DeleteUserGroup;
using UserManagement.Application.UserGroup.Commands.UpdateUesrGroup;
using UserManagement.Application.UserGroup.Queries.GetUserGroup;
using UserManagement.Application.UserGroup.Queries.GetUserGroupAutoComplete;
using UserManagement.Application.UserGroup.Queries.GetUserGroupById;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace UserManagement.Presentation.Controllers
{
    [Microsoft.AspNetCore.Components.Route("api/[controller]")]

    public class UserGroupController  : ApiControllerBase
    { 
         public UserGroupController(ISender mediator
                ) 
         : base(mediator)
        {        
        }
           [HttpGet]        
        public async Task<IActionResult> GetAllCountriesAsync([FromQuery] int PageNumber,[FromQuery] int PageSize,[FromQuery] string SearchTerm = null)
        {   
            var countries = await Mediator.Send(
            new GetUserGroupQuery
           {
                PageNumber = PageNumber, 
                PageSize = PageSize, 
                SearchTerm = SearchTerm
            });

                  
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = countries.Message,
                data = countries.Data.ToList(),
                TotalCount = countries.TotalCount,
                PageNumber = countries.PageNumber,
                PageSize = countries.PageSize
            });
        }
        [HttpGet("{id}")]     
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = "Invalid Country ID"
                });
            }            
            var result = await Mediator.Send(new GetUserGroupByIdQuery { Id = id });            
          
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }
        [HttpPost]          
        public async Task<IActionResult> CreateAsync(CreateUserGroupCommand  command)
        { 
                             
            var result = await Mediator.Send(command);
           
                return Ok(new 
                { 
                    StatusCode=StatusCodes.Status201Created,
                    message = "UserGroup created successfully", 
                    data = result
                });
           
                       
        }
        [HttpPut]      
        public async Task<IActionResult> UpdateAsync( UpdateUserGroupCommand command)
        {
           
            if (command.Id<=0)
            {
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = "Invalid Country ID"
                });
            }        
            var result = await Mediator.Send(command);
          
                return Ok(new 
                { 
                    StatusCode=StatusCodes.Status201Created,
                    message = "UserGroup updated successfully", 
                    data = result 
                });
           
        }
        [HttpDelete("{id}")]   
        public async Task<IActionResult> DeleteAsync(int id)
        {
             var command = new DeleteUserGroupCommand { Id = id };
          
            if (id <= 0)
            {
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = "Invalid Country ID"
                });
            }            
               await Mediator.Send(command);                 
           
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data =$"Country ID {id} Deleted" ,
                message = "Country Deleted Successfully"
            });
        }

        [HttpGet("by-name")]     
        public async Task<IActionResult> GetUserGroup([FromQuery] string name)
        {
            var result = await Mediator.Send(new GetUserGroupAutoCompleteQuery { SearchPattern = name });
         
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "Country List",
                data = result
            });
        } 
    }
}