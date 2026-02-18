#nullable disable
using UserManagement.Application.Country.Commands.CreateCountry;
using UserManagement.Application.Country.Commands.DeleteCountry;
using UserManagement.Application.Country.Commands.UpdateCountry;
using UserManagement.Application.Country.Queries.GetCountries;
using UserManagement.Application.Country.Queries.GetCountryAutoComplete;
using UserManagement.Application.Country.Queries.GetCountryById;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace UserManagement.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    
    public class CountryController : ApiControllerBase
    {      
         
       public CountryController(ISender mediator) 
         : base(mediator)
        {                  
        }
        [HttpGet]        
        public async Task<IActionResult> GetAllCountriesAsync([FromQuery] int PageNumber,[FromQuery] int PageSize,[FromQuery] string SearchTerm = null)
        {   
            var countries = await Mediator.Send(
            new GetCountryQuery
           {
                PageNumber = PageNumber, 
                PageSize = PageSize, 
                SearchTerm = SearchTerm
            });

                  
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = countries.Message,
                data = countries.Data.ToList(),
                TotalCount = countries.TotalCount,
                PageNumber = countries.PageNumber,
                PageSize = countries.PageSize
            });
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
            var result = await Mediator.Send(new GetCountryByIdQuery { Id = id });            
         
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }
        [HttpPost]          
        public async Task<IActionResult> CreateAsync(CreateCountryCommand  command)
        { 
                             
            var result = await Mediator.Send(command);
           
                return Ok(new 
                { 
                    StatusCode=StatusCodes.Status201Created,
                    message = "Country Created Successfully", 
                    data = result
                });
           
                       
        }
        [HttpPut]      
        public async Task<IActionResult> UpdateAsync( UpdateCountryCommand command)
        {
            
            if (command.Id<=0)
            {
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = "Invalid Country ID"
                });
            }        
            var result = await Mediator.Send(command);
           
                return Ok(new 
                { 
                    StatusCode=StatusCodes.Status201Created,
                    message = "Country Updated Successfully", 
                    data = result 
                });
            
           
        }
        [HttpDelete("{id}")]   
        public async Task<IActionResult> DeleteAsync(int id)
        {
             var command = new DeleteCountryCommand { Id = id };
           
            if (id <= 0)
            {
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = "Invalid Country ID"
                });
            }            
               await Mediator.Send(command);                 
         
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data =$"Country ID {id} Deleted" ,
                message = "Country Deleted Successfully"
            });
        }

        [HttpGet("by-name")]     
        public async Task<IActionResult> GetCountry([FromQuery] string name)
        {
            var result = await Mediator.Send(new GetCountryAutoCompleteQuery { SearchPattern = name });
       
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "Country List",
                data = result
            });
        } 
    }
}