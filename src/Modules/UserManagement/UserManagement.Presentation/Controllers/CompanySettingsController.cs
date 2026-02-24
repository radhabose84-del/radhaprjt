using UserManagement.Application.CompanySettings.Commands.CreateCompanySettings;
using UserManagement.Application.CompanySettings.Commands.UpdateCompanySettings;
using UserManagement.Application.CompanySettings.Queries.GetCompanySettingsById;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace UserManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class CompanySettingsController : ApiControllerBase
    {
        
        public CompanySettingsController(ISender mediator
        )
        : base(mediator)
        {
            
        }
          [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateCompanySettingsCommand command)
        {
            
            var createdCompanySetting = await Mediator.Send(command);
           
                return Ok(new 
                { 
                    StatusCode=StatusCodes.Status201Created,
                    message = "Company Setting created successfully",  
                    errors = "", 
                    data = createdCompanySetting  
                });
         
            
        }
        
        [HttpPut("update")]
        public async Task<IActionResult> Update(UpdateCompanySettingsCommand command)
        {
          
           var updatedCompany = await Mediator.Send(command);

          
                return Ok(new 
                {
                    StatusCode=StatusCodes.Status200OK,
                    message = "Company Setting updated successfully",
                    errors = ""
                });
           
        }
           [HttpGet]
        public async Task<IActionResult> GetByIdAsync()
        {
            
            // if (id <= 0)
            // {
            //     return BadRequest(new 
            //     { 
            //         StatusCode=StatusCodes.Status400BadRequest,
            //         Message = "Invalid Company Setting ID" 
            //     });
            // }

            var company = await Mediator.Send(new GetCompanySettingByIdQuery() {  });
            if (company == null)
            {
                return NotFound(new 
                { 
                    StatusCode=StatusCodes.Status404NotFound,
                    Message = "Company Setting not found" 
                });
            }
            return Ok(new 
            {
                StatusCode=StatusCodes.Status200OK,
                data = company.Data
            });
        }
    }
}