#nullable disable
using UserManagement.Infrastructure.Data;
using UserManagement.Application.State.Commands.CreateState;
using UserManagement.Application.State.Commands.DeleteState;
using UserManagement.Application.State.Commands.UpdateState;
using UserManagement.Application.State.Queries.GetStates;
using UserManagement.Application.State.Queries.GetStateAutoComplete;
using UserManagement.Application.State.Queries.GetStateById;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.State.Queries.GetStateByCountryId;
using Microsoft.AspNetCore.Http;

namespace UserManagement.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    
    public class StateController : ApiControllerBase
    {
        
         
        public StateController(ISender mediator) 
            : base(mediator)
        {        
           
            
        }
        [HttpGet]
        public async Task<IActionResult> GetAllStatesAsync([FromQuery] int PageNumber,[FromQuery] int PageSize,[FromQuery] string SearchTerm = null)
        {
            var states = await Mediator.Send(
            new GetStateQuery
           {
                PageNumber = PageNumber, 
                PageSize = PageSize, 
                SearchTerm = SearchTerm
            });            
            return Ok(new 
            { 
                StatusCode=StatusCodes.Status200OK, 
                message = states.Message,
                data = states.Data.ToList(),
                TotalCount = states.TotalCount,
                PageNumber = states.PageNumber,
                PageSize = states.PageSize
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            if (id <= 0)
            {                
                return BadRequest(new 
                { 
                    StatusCode=StatusCodes.Status400BadRequest,
                    message = "Invalid State ID" 
                });
            }
            var result = await Mediator.Send(new GetStateByIdQuery { Id = id });                         
            if(result is null)
            {                
                return NotFound(new 
                { 
                    StatusCode=StatusCodes.Status404NotFound,
                    message = "StateID {stateId} not found", 
                });
            }
            return Ok(new 
            {
                StatusCode=StatusCodes.Status200OK,
                data = result
            });   
        }        
        [HttpPost]   
        public async Task<IActionResult> CreateAsync(CreateStateCommand  command)
        { 
                       
            var result = await Mediator.Send(command);
                         
                return Ok(new { StatusCode=StatusCodes.Status201Created, message = "State created successfully", errors = "", data = result });
       
            
        }
        [HttpPut]
        public async Task<IActionResult> UpdateAsync(UpdateStateCommand command)
        {           
         
            if (command.CountryId<=0)
            {
                return BadRequest(
                    new
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        message = "Invalid StateID"
                    }
                );
            }

            var result = await Mediator.Send(command);
                           
                return Ok(new 
                {   StatusCode=StatusCodes.Status200OK,
                    message = "State updated successfully", 
                    City = result
                });
           
        }        
        [HttpDelete("{id}")]   
        public async Task<IActionResult> DeleteAsync(int id)
        {  
            
              var command = new DeleteStateCommand { Id = id };
                 
            if (id <= 0)
            {
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = "Invalid State ID"
                });
            }            
              var result = await Mediator.Send(command);                 
        
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data =$"State ID {id} Deleted" ,
                message = "State Deleted Successfully"
            });
        }

        [HttpGet("by-name")]  
        public async Task<IActionResult> GetState([FromQuery] string name)
        {           
            var result = await Mediator.Send(new GetStateAutoCompleteQuery {SearchPattern = name}); // Pass `searchPattern` to the constructor
         
                return Ok(new 
                {
                    StatusCode=StatusCodes.Status200OK,
                    message = "State List",
                    data = result
                });
         
        }  
        [HttpGet("by-country/{countryid}")]
        public async Task<IActionResult> GetStateByCountryId(int countryid)
        {
            if (countryid <= 0)
            {                
                return BadRequest(new 
                { 
                    StatusCode=StatusCodes.Status400BadRequest,
                    message = "Invalid State ID" 
                });
            }
            var result = await Mediator.Send(new GetStateByCountryIdQuery { Id = countryid });                         
            if(result is null)
            {                
                return NotFound(new 
                { 
                    StatusCode=StatusCodes.Status404NotFound,
                    message = "StateID {stateId} not found", 
                });
            }
            return Ok(new 
            {
                StatusCode=StatusCodes.Status200OK,
                data = result
            });   
        }   
    }
}