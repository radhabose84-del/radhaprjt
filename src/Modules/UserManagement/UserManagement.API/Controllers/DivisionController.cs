using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.Divisions.Queries.GetDivisions;
using UserManagement.Application.Divisions.Commands.CreateDivision;
using UserManagement.Application.Divisions.Queries.GetDivisionById;
using UserManagement.Application.Divisions.Commands.UpdateDivision;
using UserManagement.Application.Divisions.Commands.DeleteDivision;
using Microsoft.AspNetCore.Http;
using System.IO;
using UserManagement.Application.Divisions.Queries.GetDivisionAutoComplete;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;

namespace UserManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    
    public class DivisionController : ApiControllerBase
    {
        
        public DivisionController(ISender mediator
        ) 
        : base(mediator)
        {
        }
         [HttpGet]
        public async Task<IActionResult> GetAllDivisionsAsync([FromQuery] int PageNumber,[FromQuery] int PageSize,[FromQuery] string? SearchTerm = null)
        {
           var divisions = await Mediator.Send(
            new GetDivisionQuery
            {
                PageNumber = PageNumber, 
                PageSize = PageSize, 
                SearchTerm = SearchTerm
            });
          //  var activedivisions = divisions.Data.ToList(); 
           
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
        public async Task<IActionResult> CreateAsync(CreateDivisionCommand command)
        {
            
          
            var response = await Mediator.Send(command);
      
                return Ok(new { StatusCode=StatusCodes.Status201Created, message = "Division created successfully", errors = "", data = response });
           
            
        }
         [HttpGet("{id}")]
         [ActionName(nameof(GetByIdAsync))]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
           
            var division = await Mediator.Send(new GetDivisionByIdQuery() { Id = id});
          
            return Ok(new { StatusCode=StatusCodes.Status200OK, data = division});
        }

        [HttpPut]
        public async Task<IActionResult> Update( UpdateDivisionCommand command )
        {
          
          

             var divisionExists = await Mediator.Send(new GetDivisionByIdQuery { Id = command.Id });

             if (divisionExists == null)
             {
                 return NotFound(new { StatusCode=StatusCodes.Status404NotFound, message = $"Division ID {command.Id} not found.", errors = "" }); 
             }

             var response = await Mediator.Send(command);
            
                 return Ok(new { StatusCode=StatusCodes.Status200OK, message = "Division updated successfully", errors = "" });
      
        }


        [HttpDelete("{id}")]
        
        public async Task<IActionResult> Delete(int id)
        {
            var command = new DeleteDivisionCommand { Id = id };
           
           var updatedDivision = await Mediator.Send(command);

         
            return Ok(new { StatusCode=StatusCodes.Status200OK, message = "Division deleted successfully", errors = "" });
              
       
            
        }

         [HttpGet("by-name")]
        public async Task<IActionResult> GetDivision([FromQuery] string? name)
        {
           
           var companiesClaim = User.FindFirst("companyId")?.Value; 
           
            var divisions = await Mediator.Send(new GetDivisionAutoCompleteQuery {SearchPattern = name,Companies = companiesClaim});
            return Ok( new { StatusCode=StatusCodes.Status200OK, data = divisions });
        }
      
      
    }
}