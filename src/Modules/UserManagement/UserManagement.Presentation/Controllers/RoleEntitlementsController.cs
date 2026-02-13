using Newtonsoft.Json;
using MediatR;
using UserManagement.Infrastructure.Data;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.RoleEntitlements.Commands.CreateRoleEntitlement;
using UserManagement.Application.RoleEntitlements.Commands.UpdateRoleRntitlement;
using UserManagement.Application.RoleEntitlements.Queries.GetRoleEntitlements;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using UserManagement.Application.RoleEntitlements.Commands.DeleteRoleEntitlement;
using UserManagement.Application.RoleEntitlements.Queries.GetRoleEntitlementById;
using UserManagement.Application.RoleEntitlements.Queries.GetRolePrivileges;
using Microsoft.Extensions.Logging;

namespace UserManagement.Presentation.Controllers
{
[ApiController]
[Route("api/[controller]")]

public class RoleEntitlementsController : ApiControllerBase
{
    // public RoleEntitlementsController(ISender mediator) : base(mediator)
    // {
    // }

         
         private readonly ILogger<RoleEntitlementsController> _logger;

         
       public RoleEntitlementsController(ISender mediator, 
                             ILogger<RoleEntitlementsController> logger) 
         : base(mediator)
        {        
            
            _logger = logger;
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
            var result = await Mediator.Send(new GetRoleEntitlementByIdQuery { Id = id });            
          
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

    [HttpPost]
    public async Task<IActionResult> CreateRoleEntitlement(CreateRoleEntitlementCommand command)
    {
   
         var response = await Mediator.Send(command);
      
                
                return Ok(new 
                { 
                    StatusCode = StatusCodes.Status201Created, 
                    message = "Role Entitlement created successfully", 
                    data = response 
                });
        
             
    }

    [HttpPut]
    public async Task<IActionResult> UpdateRoleEntitlement(UpdateRoleEntitlementCommand command)
    {
          if (command.RoleId != command.RoleId)
         {
             return BadRequest(new 
             {
                 StatusCode = StatusCodes.Status400BadRequest,
                 Message = "Invalid request. All ModuleMenus must have the same RoleId as the provided RoleId."
             });
         }
           await Mediator.Send(command);
   
                return Ok(new 
                { 
                    StatusCode = StatusCodes.Status200OK, 
                    message = "Role Entitlement updated successfully",
                });
           
    }

        [HttpGet("roleprivileges/{UserId}")]
        public async Task<IActionResult> GetAllRolePrivilegesAsync(int UserId)
        {
           var roleprivileges = await Mediator.Send(
            new GetRolePrivilegesQuery
           {
                UserId = UserId
            });
            
           
            return Ok( new 
            { 
                StatusCode=StatusCodes.Status200OK,
                 data = roleprivileges.ToList()
                 });
        }
      
}
}