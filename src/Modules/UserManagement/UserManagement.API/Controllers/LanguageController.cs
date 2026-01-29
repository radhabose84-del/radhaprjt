using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Language.Commands.CreateLanguage;
using Core.Application.Language.Commands.DeleteLanguage;
using Core.Application.Language.Commands.UpdateLanguage;
using Core.Application.Language.Queries.GetLanguageAutoComplete;
using Core.Application.Language.Queries.GetLanguageById;
using Core.Application.Language.Queries.GetLanguages;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace UserManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LanguageController : ApiControllerBase
    {
       
        public LanguageController(ISender mediator
       ) 
        : base(mediator)
        {
        }
          [HttpGet]
        public async Task<IActionResult> GetAllLanguagesAsync([FromQuery] int PageNumber,[FromQuery] int PageSize,[FromQuery] string? SearchTerm = null)
        {
           var languages = await Mediator.Send(
            new GetLanguageQuery
           {
                PageNumber = PageNumber, 
                PageSize = PageSize, 
                SearchTerm = SearchTerm
            });
            
           
            return Ok( new 
            { 
                StatusCode=StatusCodes.Status200OK,
                 data = languages.Data.ToList(),
                TotalCount = languages.TotalCount,
                 PageNumber = languages.PageNumber,
                 PageSize = languages.PageSize
                 });
        }
         [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateLanguageCommand command)
        {
            
           
            var response = await Mediator.Send(command);
           
                return Ok(new { StatusCode=StatusCodes.Status201Created, message = "Language created successfully", errors = "", data = response });
  
            
        }
         [HttpGet("{id}")]
         [ActionName(nameof(GetByIdAsync))]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
           
            var language = await Mediator.Send(new GetLanguageByIdQuery() { Id = id});
          
            return Ok(new { StatusCode=StatusCodes.Status200OK, data = language});
        }

        [HttpPut]
        public async Task<IActionResult> Update( UpdateLanguageCommand command )
        {
            
          

             var languageExists = await Mediator.Send(new GetLanguageByIdQuery { Id = command.Id });

             if (languageExists == null)
             {
                 return NotFound(new { StatusCode=StatusCodes.Status404NotFound, message = $"Language ID {command.Id} not found.", errors = "" }); 
             }

              await Mediator.Send(command);
            
                 return Ok(new { StatusCode=StatusCodes.Status200OK, message = "Language updated successfully", errors = "" });
             
            
        }


        [HttpPut("{id}")]
        
        public async Task<IActionResult> Delete(int id)
        {
           
            await Mediator.Send(new DeleteLanguageCommand { Id = id });

          
            return Ok(new { StatusCode=StatusCodes.Status200OK, message = "Language deleted successfully", errors = "" });
          
            
        }
         [HttpGet("by-name")]
        public async Task<IActionResult> GetLanguage([FromQuery] string? name)
        {
           
            var languages = await Mediator.Send(new GetLanguageAutoCompleteQuery {SearchPattern = name});
            return Ok( new { StatusCode=StatusCodes.Status200OK, data = languages });
        }
      
    }
}