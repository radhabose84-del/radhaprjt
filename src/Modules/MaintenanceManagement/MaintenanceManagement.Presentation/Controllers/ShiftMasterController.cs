#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.ShiftMasters.Commands.CreateShiftMaster;
using MaintenanceManagement.Application.ShiftMasters.Commands.DeleteShiftMaster;
using MaintenanceManagement.Application.ShiftMasters.Commands.UpdateShiftMaster;
using MaintenanceManagement.Application.ShiftMasters.Queries.GetShiftMaster;
using MaintenanceManagement.Application.ShiftMasters.Queries.GetShiftMasterAutoComplete;
using MaintenanceManagement.Application.ShiftMasters.Queries.GetShiftMasterById;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MaintenanceManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class ShiftMasterController : ApiControllerBase
    {

        public ShiftMasterController(ISender mediator)
        : base(mediator)
        {
            
        }
           [HttpGet]
        public async Task<IActionResult> GetAllShiftMastersAsync([FromQuery] int PageNumber,[FromQuery] int PageSize,[FromQuery] string SearchTerm = null)
        {
           var divisions = await Mediator.Send(
            new GetShiftMasterQuery
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
        public async Task<IActionResult> CreateAsync(CreateShiftMasterCommand command)
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
             

            return BadRequest( new 
            { 
                StatusCode=StatusCodes.Status400BadRequest, 
                message = response.Message, 
                errors = "" 
            }); 
            
        }
         [HttpGet("{id}")]
         [ActionName(nameof(GetByIdAsync))]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
               
            var shiftMaster = await Mediator.Send(new GetShiftMasterByIdQuery { Id = id});
          
             if(shiftMaster == null)
            {
                return NotFound( new 
                { 
                    StatusCode=StatusCodes.Status404NotFound, 
                    message = $"Shift Master ID {id} not found.", 
                    errors = "" 
                });
            }
            return Ok(new 
            { 
                StatusCode=StatusCodes.Status200OK, 
                data = shiftMaster.Data
            });
        }

        [HttpPut]
        public async Task<IActionResult> Update( UpdateShiftMasterCommand command )
        {

             var response = await Mediator.Send(command);
             if(response.IsSuccess)
             {
                 return Ok(new 
                 { 
                    StatusCode=StatusCodes.Status200OK, 
                    message = response.Message, 
                    errors = "" 
                });
             }
            
           

            return BadRequest( new 
            { 
                StatusCode=StatusCodes.Status400BadRequest, 
                message = response.Message, 
                errors = "" 
            }); 
        }


        [HttpDelete("{id}")]
        
        public async Task<IActionResult> Delete(int id)
        {
            var command = new DeleteShiftMasterCommand { Id = id };
          
           var updatedShiftMaster = await Mediator.Send(command);

           if(updatedShiftMaster.IsSuccess)
           {
            return Ok(new 
            { 
                StatusCode=StatusCodes.Status200OK, 
                message = updatedShiftMaster.Message, 
                errors = "" 
            });
              
           }

            return BadRequest(new 
            { 
                StatusCode=StatusCodes.Status400BadRequest, 
                message = updatedShiftMaster.Message, 
                errors = "" 
            });
            
        }

         [HttpGet("by-name")]
        public async Task<IActionResult> GetShiftMaster([FromQuery] string name)
        {
           
           var shiftMaster = await Mediator.Send(new GetShiftMasterAutoCompleteQuery {SearchPattern = name});
            return Ok( new 
            { 
                StatusCode=StatusCodes.Status200OK, 
                data = shiftMaster.Data 
            });
        }
    }
}