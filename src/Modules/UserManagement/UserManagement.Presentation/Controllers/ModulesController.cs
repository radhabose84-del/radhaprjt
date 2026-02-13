using MediatR;
using UserManagement.Infrastructure.Data;
using UserManagement.Application.Modules.Commands.UpdateModule;
using UserManagement.Application.Modules.Commands.CreateModule;
using UserManagement.Application.Modules.Queries.GetModules;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Application.Modules.Commands.DeleteModule;
using Microsoft.AspNetCore.Authorization;
using UserManagement.Application.Modules.Queries.GetModuleById;
using UserManagement.Application.Modules.Queries.GetModuleAutoComplete;
using Microsoft.Extensions.Logging;

namespace UserManagement.Presentation.Controllers
{
[ApiController]
[Route("api/[controller]")]

    public class ModulesController : ApiControllerBase
    {
    // public ModulesController(ISender mediator) : base(mediator)
    // {
    // }
  
         private readonly ILogger<ModulesController> _logger;

         
       public ModulesController(ISender mediator, 
                             ILogger<ModulesController> logger) 
         : base(mediator)
        {        
            _logger = logger;

             
        }

    [HttpPost]
    public async Task<IActionResult> CreateModule([FromBody] CreateModuleCommand createModuleCommand)
    {
   

        var response = await Mediator.Send(createModuleCommand);
        
                _logger.LogInformation($"Module {createModuleCommand.ModuleName} created successfully.");

                return Ok(new { StatusCode = StatusCodes.Status201Created, message = "Module created successfully", data = response });
       
    }

    [HttpGet]
    public async Task<IActionResult> GetAllModuleAsync([FromQuery] int PageNumber,[FromQuery] int PageSize,[FromQuery] string? SearchTerm = null)
    {
        var modules = await Mediator.Send(new GetModulesQuery
            {
                PageNumber = PageNumber, 
                PageSize = PageSize, 
                SearchTerm = SearchTerm
            });
        _logger.LogInformation($"Total {modules.Data.Count} modules listed successfully.");
          return Ok( new 
            { 
                StatusCode=StatusCodes.Status200OK, 
                data = modules.Data,
                TotalCount = modules.TotalCount,
                PageNumber = modules.PageNumber,
                PageSize = modules.PageSize
                });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
            
        var module = await Mediator.Send(new GetModuleByIdQuery { Id = id });

        if (module is null)
        {
            _logger.LogWarning($"Module not found for ID {id}.");

           return NotFound(new { StatusCode = StatusCodes.Status404NotFound, message = $"Module ID {id} not found." });
        }
           _logger.LogWarning("Module Listed successfully: {Modulename}", module);

        return Ok(new { StatusCode = StatusCodes.Status200OK, data = module });
    }
    [HttpPut]
    public async Task<IActionResult> UpdateModule([FromBody] UpdateModuleCommand updateModuleCommand)
    {
     

         await Mediator.Send(updateModuleCommand);
     
                _logger.LogInformation($"Module {updateModuleCommand.ModuleName} updated successfully.");

                return Ok(new { StatusCode = StatusCodes.Status200OK, message = "Module updated successfully" });
       
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteModule(int id)
    {
         var command = new DeleteModuleCommand { ModuleId = id };
            
          
        if (id <= 0)
        {
            return BadRequest(new
           {
                StatusCode = StatusCodes.Status400BadRequest,
                message = "Invalid Module ID"
            });
        }
         await Mediator.Send(command);                 
         
            _logger.LogInformation($"Module {id} deleted successfully.");

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data =$"Module ID {id} Deleted" 
            });
    }
     [HttpGet("by-name")]
        public async Task<IActionResult> GetModule([FromQuery] string? name)
        {
           
            var modules = await Mediator.Send(new GetModuleAutoCompleteQuery {SearchPattern = name});
            return Ok( new { StatusCode=StatusCodes.Status200OK, data = modules });
        }


    }
}