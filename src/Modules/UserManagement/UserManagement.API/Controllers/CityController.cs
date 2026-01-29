using Core.Application.City.Commands.CreateCity;
using Core.Application.City.Commands.DeleteCity;
using Core.Application.City.Commands.UpdateCity;
using Core.Application.City.Queries.GetCities;
using Core.Application.City.Queries.GetCityAutoComplete;
using Core.Application.City.Queries.GetCityById;
using Core.Application.City.Queries.GetCityByStateId;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace UserManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    
    public class CityController : ApiControllerBase
    {
         
         
       public CityController(ISender mediator) 
         : base(mediator)
        {           
             
        }
        [HttpGet]                
        public async Task<IActionResult> GetAllCitiesAsync([FromQuery] int PageNumber,[FromQuery] int PageSize,[FromQuery] string? SearchTerm = null)
        {            
            var cities = await Mediator.Send(
            new GetCityQuery
           {
                PageNumber = PageNumber, 
                PageSize = PageSize, 
                SearchTerm = SearchTerm
            });

            return Ok(new 
            { 
                StatusCode=StatusCodes.Status200OK, 
                message = cities.Message,
                data = cities.Data.ToList(),
                TotalCount = cities.TotalCount,
                PageNumber = cities.PageNumber,
                PageSize = cities.PageSize
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
                    message = "Invalid City ID" 
                });
            }
            var result = await Mediator.Send(new GetCityByIdQuery { Id = id });            
           
            return Ok(new 
            {
                StatusCode=StatusCodes.Status200OK,
                data = result
            });   
        }

        [HttpPost]               
        public async Task<IActionResult> CreateAsync(CreateCityCommand  command)
        { 
              
            var result = await Mediator.Send(command);
           
                return Ok(new 
                { 
                    StatusCode=StatusCodes.Status201Created,
                    message = "City Created Successfully", 
                    data = result
                });
           
        }
        [HttpPut]        
        public async Task<IActionResult> UpdateAsync(UpdateCityCommand command)
        {         
          
            if (command.StateId<=0)
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
                    message = "City Updated Successfully", 
                    City = result
                });
            
                
        }
        [HttpDelete("{id}")]        
        public async Task<IActionResult> DeleteAsync(int id)
        {             
            if (id <= 0)
            {
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = "Invalid City ID"
                });
            }            
              var result = await Mediator.Send(new DeleteCityCommand { Id = id });                 
         
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data =$"City ID {id} Deleted" ,
                message = result
            });
        }
             
        [HttpGet("by-name")]  
        public async Task<IActionResult> GetCity([FromQuery] string? name)
        {          
            var result = await Mediator.Send(new GetCityAutoCompleteQuery {SearchPattern = name}); // Pass `searchPattern` to the constructor
          
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "City List",
                data = result
            });
        }    
        [HttpGet("by-state/{stateid}")]
        public async Task<IActionResult> GetStateByCountryId(int stateid)
        {
            if (stateid <= 0)
            {                
                return BadRequest(new 
                { 
                    StatusCode=StatusCodes.Status400BadRequest,
                    message = "Invalid State ID" 
                });
            }
            var result = await Mediator.Send(new GetCityByStateIdQuery { Id = stateid });                         
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