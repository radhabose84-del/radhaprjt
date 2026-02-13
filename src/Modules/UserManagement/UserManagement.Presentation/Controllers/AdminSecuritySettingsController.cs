using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Infrastructure.Data;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.AuditLog.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FluentValidation;
using UserManagement.Application.AdminSecuritySettings.Queries.GetAdminSecuritySettings;
using UserManagement.Application.AdminSecuritySettings.Commands.CreateAdminSecuritySettings;
using UserManagement.Application.AdminSecuritySettings.Commands.UpdateAdminSecuritySettings;
using UserManagement.Application.AdminSecuritySettings.Commands.DeleteAdminSecuritySettings;
using AutoMapper;
using UserManagement.Domain.Entities;
using UserManagement.Application.AdminSecuritySettings.Queries.GetAdminSecuritySettingsById;
using Microsoft.AspNetCore.Http;



namespace UserManagement.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminSecuritySettingsController : ApiControllerBase
    {
     
        
        private readonly ILogger<AdminSecuritySettingsController> _logger;



        public AdminSecuritySettingsController (ISender mediator, 
         ILogger<AdminSecuritySettingsController> logger)  : base(mediator)
        {
          
             _logger = logger;

             
        }

         [HttpGet]
                public async Task<IActionResult> GetAllAdminSecuritySettingsAsync([FromQuery] int PageNumber, [FromQuery] int PageSize, [FromQuery] string? SearchTerm = null)
        {
            _logger.LogInformation("Fetching All Admin Security Settings Request started.");

            var adminSecuritySettings = await Mediator.Send(new GetAdminSecuritySettingsQuery
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                SearchTerm = SearchTerm
            });

            if (adminSecuritySettings.Data == null || !adminSecuritySettings.Data.Any())
            {
                _logger.LogWarning("No admin security settings found in the database. Total count: {Count}", adminSecuritySettings?.Data?.Count ?? 0);

                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = adminSecuritySettings.Message
                });
            }

            _logger.LogInformation("Admin security settings retrieved successfully.");

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                Data = adminSecuritySettings.Data
            });
        }

   
        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
            {
                _logger.LogInformation($"Starting GetByIdAsync request for Admin Security Setting with ID: {id}");

                
                    // Fetch the admin security setting by ID
                    var adminSecuritySetting = await Mediator.Send(new GetAdminSecuritySettingsByIdQuery { Id = id });

                  

                    _logger.LogInformation($"Admin Security Setting with ID {id} retrieved successfully.");

                    return Ok(new
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Data = adminSecuritySetting
                    });
                
            
            }

            

         [HttpPost]        
        public async Task<IActionResult>CreateAsync([FromBody] CreateAdminSecuritySettingsCommand createAdminSecuritySettingscmd)
        {

               _logger.LogInformation($"Create AdminSecuritySettings request started with data: {createAdminSecuritySettingscmd}");

      

            // Process the command
             await Mediator.Send(createAdminSecuritySettingscmd);
         
                    

                    return Ok(new
                    {
                        StatusCode = StatusCodes.Status201Created,
                        Message = "Admin Security Settings created successfully",
                        
                    });
                
        
        }
         
           [HttpPut]
            public async Task<IActionResult> UpdateAsync(int id, UpdateAdminSecuritySettingsCommand updateadminsecuritycommand)
            {
                 _logger.LogInformation($"Starting UpdateAsync for Admin Security Settings with ID: {id}.");
            // Validate the command
    

            // Check for ID mismatch
            if (id != updateadminsecuritycommand.Id)
            {
                _logger.LogWarning($"Admin Security Settings ID mismatch. URL ID: {id}, Command ID: {updateadminsecuritycommand.Id}" );
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Admin Security Settings ID mismatch"
                });
            }

            // Process the update command
            _logger.LogInformation("Sending UpdateAdminSecuritySettingsCommand for processing.");
             await Mediator.Send(updateadminsecuritycommand);

                _logger.LogInformation($"Admin Security Settings with ID: {id} updated successfully.");
                return Ok(new
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Updated Successfully"
                    
                });
           
                
            }
            
          [HttpDelete("{id}")]       
        public async Task<IActionResult>Delete( int id)
        {
               // Check if the department exists
                var department = await Mediator.Send(new GetAdminSecuritySettingsByIdQuery { Id = id });
                if (department is null)
                {
                    _logger.LogWarning($"Admin Security Settings  with ID {id} not found." );

                    return NotFound(new
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Admin Security Settings  not found"
                    });
                }

                _logger.LogInformation($"Admin Security Settings  with ID {id} found. Proceeding with deletion.");

                // Attempt to delete the department
                 await Mediator.Send(new DeleteAdminSecuritySettingsCommand { Id = id });

            
                    _logger.LogInformation($"Admin Security Settings  with ID {id} deleted successfully.");

                    return Ok(new
                    {
                        Message = "Admin Security Settings  deleted successfully",
                        StatusCode = StatusCodes.Status200OK
                      
                    });
                

               
          
        }


    }
}