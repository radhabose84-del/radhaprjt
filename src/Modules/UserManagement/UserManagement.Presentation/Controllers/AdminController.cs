using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Application.EntityLevelAdmin.Commands.CreateEntityLevelAdmin;
using UserManagement.Application.EntityLevelAdmin.Commands.ResetPassword;
using UserManagement.Application.EntityLevelAdmin.Commands.SendOTP;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Presentation.Validation.Admin;
using Microsoft.AspNetCore.Http;

namespace UserManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class AdminController : ApiControllerBase
    {
        
        public AdminController(ISender mediator) 
        : base(mediator)
        {
            
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateEntityLevelAdminCommand command)
        {
            
                 await Mediator.Send(command);
         
                return Ok(new 
                { 
                    StatusCode=StatusCodes.Status201Created, 
                    message = "Admin created successfully",
                });
           
            
        }

         [HttpPost("SendOTP")]
        public async Task<IActionResult> SendOTP(SendOTPCommand command)
        {
             
            var response = await Mediator.Send(command);
           
                return Ok(new 
                { 
                    StatusCode=StatusCodes.Status200OK, 
                    message = "OTP sent successfully",
                    data =response
                });
          
            
        }
         [HttpPut("SetAdminPassword")]
        public async Task<IActionResult> SetAdminPassword(ResetPasswordCommand command)
        {
          
            
             await Mediator.Send(command);
       
                return Ok(new 
                { 
                    StatusCode=StatusCodes.Status200OK, 
                    message = "Password reset successfully",
                });
           
            
        }
    }
}