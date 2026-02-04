using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Application.CustomFields.Commands.CreateCustomField;
using UserManagement.Application.CustomFields.Commands.DeleteCustomField;
using UserManagement.Application.CustomFields.Commands.UpdateCustomField;
using UserManagement.Application.CustomFields.Queries.GetCustomField;
using UserManagement.Application.CustomFields.Queries.GetCustomFieldById;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace UserManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomFieldController : ApiControllerBase
    {
        public CustomFieldController(ISender mediator 
                                    ) 
        : base(mediator)
        {
           
        }
           [HttpGet]
        public async Task<IActionResult> GetAllCustomFieldsAsync([FromQuery] int PageNumber,[FromQuery] int PageSize,[FromQuery] string? SearchTerm = null)
        {
           var divisions = await Mediator.Send(
            new GetCustomFieldQuery
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
        public async Task<IActionResult> CreateAsync(CreateCustomFieldCommand command)
        {
            
        
            var response = await Mediator.Send(command);
           
                return Ok(new 
                { 
                    StatusCode=StatusCodes.Status201Created, 
                    message = "CustomField created successfully", 
                    errors = "", 
                    data = response 
                });
          
            
        }
         [HttpGet("{id}")]
         [ActionName(nameof(GetByIdAsync))]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
               
            var customField = await Mediator.Send(new GetCustomFieldByIdQuery { Id = id});
          
         
            return Ok(new 
            { 
                StatusCode=StatusCodes.Status200OK, 
                data = customField
            });
        }

        [HttpPut]
        public async Task<IActionResult> Update( UpdateCustomFieldCommand command )
        {
          

             var response = await Mediator.Send(command);
           
                 return Ok(new 
                 { 
                    StatusCode=StatusCodes.Status200OK, 
                    message = "CustomField updated successfully", 
                    errors = "" 
                });
            
        }


        [HttpDelete("{id}")]
        
        public async Task<IActionResult> Delete(int id)
        {
            var command = new DeleteCustomFieldCommand { Id = id };
            
            await Mediator.Send(command);

          
            return Ok(new 
            { 
                StatusCode=StatusCodes.Status200OK, 
                message = "CustomField deleted successfully", 
                errors = "" 
            });
              
         
            
        }
    }
}