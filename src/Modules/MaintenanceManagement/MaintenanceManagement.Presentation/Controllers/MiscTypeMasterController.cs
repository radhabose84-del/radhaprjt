using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MediatR;
using MaintenanceManagement.Infrastructure.Data;
using MaintenanceManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using FluentValidation;
using MaintenanceManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMasterById;
using MaintenanceManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMasterAutoComplete;
using MaintenanceManagement.Application.MiscTypeMaster.Command.CreateMiscTypeMaster;
using MaintenanceManagement.Infrastructure.Repositories.MiscTypeMaster;
using MaintenanceManagement.Application.MiscTypeMaster.Command.UpdateMiscTypeMaster;
using MaintenanceManagement.Application.Common.Interfaces.IMiscTypeMaster;
using MaintenanceManagement.Application.MiscTypeMaster.Command.DeleteMiscTypeMaster;
using Microsoft.AspNetCore.Http;

namespace MaintenanceManagement.Presentation.Controllers
{
   [Route("api/maintenance/[controller]")]
    public class MiscTypeMasterController : ApiControllerBase
    {

          
          
          public MiscTypeMasterController(ISender mediator ):base(mediator) 
        
          {
          }      
    
      [HttpGet] 
          public async Task<IActionResult> GetAllMiscTypeMasterAsync([FromQuery] int PageNumber,[FromQuery] int PageSize,[FromQuery] string? SearchTerm = null)
        {
            var misctypemaster = await Mediator.Send(
            new GetMiscTypeMasterQuery
            {
                PageNumber = PageNumber, 
                PageSize = PageSize, 
                SearchTerm = SearchTerm
            });
           // var activecompanies = companies.Data.ToList(); 

            return Ok(new 
            { 
                StatusCode=StatusCodes.Status200OK, 
                data = misctypemaster.Data,
                TotalCount = misctypemaster.TotalCount,
                PageNumber = misctypemaster.PageNumber,
                PageSize = misctypemaster.PageSize
            });
        }

         [HttpGet("{id}")]
         [ActionName(nameof(GetByIdAsync))]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
           
            var misctypemaster = await Mediator.Send(new GetMiscTypeMasterByIdQuery() { Id = id});
          
             if(misctypemaster.IsSuccess)
            {
                 return Ok(new { StatusCode=StatusCodes.Status200OK, data = misctypemaster.Data,message=misctypemaster.Message});
            }

            return NotFound( new { StatusCode=StatusCodes.Status404NotFound, message = $"MiscTypeMaster ID {id} not found.", errors = "" });
           
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetMiscTypeMaster([FromQuery] string? name)
        {
          
            var misctypemaster = await Mediator.Send(new GetMiscTypeMasterAutoCompleteQuery {SearchPattern = name});
            if(misctypemaster.IsSuccess)
            {
            return Ok( new { StatusCode=StatusCodes.Status200OK, data = misctypemaster.Data });
            }
            return NotFound( new { StatusCode=misctypemaster.Message}) ;
        }

          [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateMiscTypeMasterCommand command)
        {
            
          
            var response = await Mediator.Send(command);
            if(response.IsSuccess)
            {                             
                return Ok(new 
                {
                     StatusCode=StatusCodes.Status201Created,
                 message = response.Message,
                  errors = "",
                  data = response.Data 
                  });
            }
             
            
            return BadRequest( new {
                 StatusCode=StatusCodes.Status400BadRequest,
                  message = response.Message, 
                  errors = "" 
                  }); 
            
        } 


        [HttpPut]
        public async Task<IActionResult> Update(UpdateMiscTypeMasterCommand command )
        {
                 

             var misctypeExists = await Mediator.Send(new GetMiscTypeMasterByIdQuery { Id = command.Id });

             if (misctypeExists == null)
             {
                 return NotFound(new { StatusCode=StatusCodes.Status404NotFound, message = $"MiscTypeMaster ID {command.Id} not found.", errors = "" }); 
             }

             var response = await Mediator.Send(command);
             if(response.IsSuccess)
             {
                 return Ok(new { StatusCode=StatusCodes.Status200OK, message = response.Message, errors = "" });
             }
            
           

            return BadRequest( new { StatusCode=StatusCodes.Status400BadRequest, message = response.Message, errors = "" }); 
        }

         [HttpDelete("{id}")]
        
        public async Task<IActionResult> Delete(int id)
        {
           
           var updatedDivision = await Mediator.Send(new DeleteMiscTypeMasterCommand { Id = id });

           if(updatedDivision.IsSuccess)
           {
            return Ok(new { StatusCode=StatusCodes.Status200OK, message = updatedDivision.Message, errors = "" });
              
           }

            return BadRequest(new { StatusCode=StatusCodes.Status400BadRequest, message = updatedDivision.Message, errors = "" });
            
        }
       
      





    }
}