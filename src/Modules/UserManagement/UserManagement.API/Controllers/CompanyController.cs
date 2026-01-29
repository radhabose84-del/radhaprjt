using MediatR;
using Microsoft.AspNetCore.Mvc;
using Core.Application.Companies.Queries.GetCompanies;
using Core.Application.Companies.Commands.CreateCompany;
using Core.Application.Companies.Queries.GetCompanyById;
using Core.Application.Companies.Commands.UpdateCompany;
using Core.Application.Companies.Commands.DeleteCompany;
using Core.Application.Companies.Queries.GetCompanyAutoComplete;
using FluentValidation;
using Core.Application.Companies.Commands.UploadFileCompany;
using Core.Application.Companies.Commands.DeleteFileCompany;
using Microsoft.AspNetCore.Authorization;

namespace UserManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    
    public class CompanyController : ApiControllerBase
    {
        

        public CompanyController(ISender mediator) 
        : base(mediator)
        {
        }
        
        [HttpGet]
        
        public async Task<IActionResult> GetAllCompaniesAsync([FromQuery] int PageNumber,[FromQuery] int PageSize,[FromQuery] string? SearchTerm = null)
        {
            var companies = await Mediator.Send(
            new GetCompanyQuery
            {
                PageNumber = PageNumber, 
                PageSize = PageSize, 
                SearchTerm = SearchTerm
            });
           // var activecompanies = companies.Data.ToList(); 

            return Ok(new 
            { 
                StatusCode=StatusCodes.Status200OK, 
                data = companies.Data,
                TotalCount = companies.TotalCount,
                PageNumber = companies.PageNumber,
                PageSize = companies.PageSize
            });
        }
         [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateCompanyCommand command)
        {
            
           
            var createdCompany = await Mediator.Send(command);
          
                return Ok(new 
                { 
                    StatusCode=StatusCodes.Status201Created,
                    message = "Company created successfully",  
                    errors = "", 
                    data = createdCompany  
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
                    Message = "Invalid Company ID" 
                });
            }

            var company = await Mediator.Send(new GetCompanyByIdQuery() { CompanyId = id });
        
            return Ok(new 
            {
                StatusCode=StatusCodes.Status200OK,
                data = company
            });
        }

        [HttpPut]
        public async Task<IActionResult> Update(UpdateCompanyCommand command)
        {
            
           
            var companyExists = await Mediator.Send(new GetCompanyByIdQuery { CompanyId = command.Company.Id });

             if (companyExists == null)
             {
                 return NotFound(new 
                 { 
                    StatusCode=StatusCodes.Status404NotFound, 
                    message = $"Company ID {command.Company.Id} not found.", 
                    errors = "" 
                }); 
             }
            await Mediator.Send(command);

                return Ok(new 
                {
                    StatusCode=StatusCodes.Status200OK,
                    message = "Company updated successfully",
                    errors = ""
                });
          
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
           
            var command = new DeleteCompanyCommand { Id = id };
            await Mediator.Send(command);

          
                return Ok(new 
                {
                    StatusCode=StatusCodes.Status200OK,
                    message = "Company deleted successfully",
                    errors = ""
                });
           
        }
         [HttpGet("by-name")]
        public async Task<IActionResult> GetCompany([FromQuery] string? name)
        {
            
            var companies = await Mediator.Send(new GetCompanyAutoCompleteQuery {SearchPattern = name});
            return Ok(new 
            { StatusCode=StatusCodes.Status200OK, 
            data = companies 
            });
        }
        [HttpPost("upload-logo")]
        public async Task<IActionResult> UploadLogo(UploadFileCompanyCommand uploadFileCompanyCommand)
        {
            
            var file = await Mediator.Send(uploadFileCompanyCommand);
               
           return Ok(new 
            { 
                StatusCode=StatusCodes.Status200OK, 
                message = "File uploaded successfully", 
                data = file,
                errors = "" 
            });
              

        }
        [HttpDelete("delete-logo")]
        public async Task<IActionResult> DeleteLogo(DeleteFileCompanyCommand deleteFileCompanyCommand)
        {
             await Mediator.Send(deleteFileCompanyCommand);
         
            return Ok(new 
            { 
                StatusCode=StatusCodes.Status200OK, 
                message = "File deleted successfully", 
                errors = "" 
            });
        }
     
    }
}