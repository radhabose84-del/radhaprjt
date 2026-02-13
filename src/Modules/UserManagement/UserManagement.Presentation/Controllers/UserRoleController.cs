using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.UserRole.Queries.GetRole;
using UserManagement.Application.UserRole.Queries.GetRoleById;
using UserManagement.Application.UserRole.Commands.CreateRole;
using UserManagement.Application.UserRole.Commands.DeleteRole;
using UserManagement.Application.UserRole.Commands.UpdateRole;
using UserManagement.Application.UserRole.Queries.GetRolesAutocomplete;
using UserManagement.Application.Common.Interfaces;
using FluentValidation;
using UserManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using UserManagement.Application.Common.Interfaces.IUser;
using UserManagement.Application.Common.HttpResponse;
using Microsoft.Extensions.Logging;

namespace UserManagement.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    
    public class UserRoleController : ApiControllerBase
    {
         private readonly ILogger<UserRoleController> _logger;

         private readonly IUserCommandRepository  _userCommandRepository;
        public UserRoleController(ISender mediator    ,
        IUserCommandRepository userCommandRepository,
        ILogger<UserRoleController> logger) : base(mediator)
        {
            _userCommandRepository= userCommandRepository;
             _logger = logger;

        }
       
        [HttpGet]
        public async Task<IActionResult> GetAllRoleAsync([FromQuery] int PageNumber, [FromQuery] int PageSize, [FromQuery] string? SearchTerm = null)
            {
                _logger.LogInformation("Fetching all user roles request started.");

                var userRoles = await Mediator.Send(new GetRoleQuery
                {
                    PageNumber = PageNumber,
                    PageSize = PageSize,
                    SearchTerm = SearchTerm
                });

                if (userRoles == null || userRoles.Data == null || !userRoles.Data.Any())
                {
                    _logger.LogWarning("No user role records found.");
                    return NotFound(new
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "No user roles found."
                    });
                }

                _logger.LogInformation("User roles retrieved successfully.");
                return Ok(new
                {
                    StatusCode = StatusCodes.Status200OK,
                    Data = userRoles.Data,
                    TotalCount = userRoles.TotalCount,
                    PageNumber = PageNumber,
                    PageSize = PageSize
                });
            }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {


            _logger.LogInformation($"Fetching role with ID: {id}");

             var userRole = await Mediator.Send(new GetRoleByIdQuery { Id = id });
          
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                Data = userRole
            });

           

        }
        [HttpGet("by-name")] 
        public async Task<IActionResult> GetRoles([FromQuery] string? name)
            {
                _logger.LogInformation("Starting GetAllUserRoleAutoCompleteSearchAsync with search pattern: {SearchPattern}", name);

                var query = new GetRolesAutocompleteQuery { SearchTerm = name ?? string.Empty };
                var result = await Mediator.Send(query);

              
                    _logger.LogInformation("User Role found for search pattern: {SearchPattern}. Returning data.", name);

                    return Ok(new
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Data = result
                    });
             
            }

      

        [HttpPost]
        public async Task<IActionResult>CreateAsync(CreateRoleCommand createRoleCommand)
        {

                   _logger.LogInformation($"Create User Role request started with data: {createRoleCommand}");

         

            // Process the command
            var createuserrole = await Mediator.Send(createRoleCommand);
           
                return Ok(new
                {
                    StatusCode = StatusCodes.Status201Created,
                    Message = "User Role created successfully",
                    data = createuserrole
                    
                });
         
            
               
        }

        [HttpDelete("{id}")]         
        public async Task<IActionResult> DeleteAsync( int id )
        {
              _logger.LogInformation($"Delete User Role request started with ID: {id}");
              
            var command = new DeleteRoleCommand { Id = id };
           
                var userRole = await Mediator.Send(command);
                if (userRole == null)
                {
                    _logger.LogWarning($"User Role with ID {id} not found.");

                    return NotFound(new
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "User Role not found"
                    });
                }

                _logger.LogInformation($"User Role with ID {id} found. Proceeding with deletion.");

                // Attempt to delete the department
                 await Mediator.Send(new  DeleteRoleCommand {Id=id});

               
                    _logger.LogInformation($"User Role with ID {id} deleted successfully.");

                    return Ok(new
                    {
                        Message = "User Role deleted successfully",
                        StatusCode = StatusCodes.Status200OK
                      
                    });
              

        
        }
       
  

    [HttpPut]
    public async Task<IActionResult> UpdateAsync( UpdateRoleCommand updateRolecommand)
    {      
               _logger.LogInformation($"Update User Role  request started with data: {updateRolecommand}");

            // Check if the department exists
            var department = await Mediator.Send(new GetRoleByIdQuery { Id = updateRolecommand.Id });
            if (department == null)
            {
                _logger.LogWarning($"User Role with ID {updateRolecommand.Id} not found.");

                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "User Role  not found"
                });
            }
       

          
             await Mediator.Send(updateRolecommand);
          
                _logger.LogInformation($"User Role  with ID {updateRolecommand.Id} updated successfully." );

                return Ok(new
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "User Role  updated successfully"
                  
                });
                   
    }


    }
}