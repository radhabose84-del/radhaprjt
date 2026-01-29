using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.ShiftMasterDetails.Commands.CreateShiftMasterDetail;
using MaintenanceManagement.Application.ShiftMasterDetails.Commands.DeleteShiftMasterDetail;
using MaintenanceManagement.Application.ShiftMasterDetails.Commands.UpdateShiftMasterDetail;
using MaintenanceManagement.Application.ShiftMasterDetails.Queries.GetShiftMasterDetail;
using MaintenanceManagement.Application.ShiftMasterDetails.Queries.GetShiftMasterDetailById;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MaintenanceManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShiftMasterDetailController : ApiControllerBase
    {
        
        public ShiftMasterDetailController(ISender mediator) 
        : base(mediator)
        {
            
        }

           [HttpGet]
        public async Task<IActionResult> GetAllShiftMasterDetailsAsync([FromQuery] int PageNumber,[FromQuery] int PageSize,[FromQuery] string? SearchTerm = null)
        {
           var response = await Mediator.Send(
            new GetShiftMasterDetailQuery
            {
                PageNumber = PageNumber, 
                PageSize = PageSize, 
                SearchTerm = SearchTerm
            });
           
            return Ok( new 
            { 
                StatusCode=StatusCodes.Status200OK, 
                data = response.Data,
                TotalCount = response.TotalCount,
                PageNumber = response.PageNumber,
                PageSize = response.PageSize
                });
        }
         
         [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateShiftMasterDetailCommand command)
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
        public async Task<IActionResult> Update( UpdateShiftMasterDetailCommand command )
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
            var command = new DeleteShiftMasterDetailCommand { Id = id };
          
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
    }
}