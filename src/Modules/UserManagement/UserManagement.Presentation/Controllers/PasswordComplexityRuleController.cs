 
 using UserManagement.Infrastructure.Data;
using UserManagement.Application.PasswordComplexityRule.Commands.UpdatePasswordComplexityRule;

//using UserManagement.Application.PasswordComplexityRule.Commands.UpdatePasswordComplexityRule;
using UserManagement.Application.PwdComplexityRule.Commands.CreatePasswordComplexityRule;
using UserManagement.Application.PwdComplexityRule.Commands.DeletePasswordComplexityRule;
using UserManagement.Application.PwdComplexityRule.Queries;
using UserManagement.Application.PwdComplexityRule.Queries.GetPwdComplexityRuleAutoComplete;
using UserManagement.Application.PwdComplexityRule.Queries.GetPwdComplexityRuleById;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace UserManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class PasswordComplexityRuleController :ApiControllerBase
    {
       
         private readonly ILogger<PasswordComplexityRuleController> _logger;
         public PasswordComplexityRuleController(ISender mediator  ,ILogger<PasswordComplexityRuleController> logger ) : base(mediator)
        {                      
             
             _logger = logger;
        }

        [HttpGet]
    //    public async Task<IActionResult> GetPasswordComplexityAsync()
        public async Task<IActionResult> GetPasswordComplexityAsync([FromQuery] int PageNumber,[FromQuery] int PageSize,[FromQuery] string? SearchTerm = null)
        {
               
                _logger.LogInformation("Starting GetPasswordComplexityAsync request.");
                var pwdComplexityRules = await Mediator.Send(new GetPwdRuleQuery
                {
                    PageNumber = PageNumber, 
                    PageSize = PageSize, 
                    SearchTerm = SearchTerm
                });

            if (pwdComplexityRules == null )
            {
                _logger.LogWarning("No password complexity rules found.");
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "No password complexity rules found."
                });
            }

            _logger.LogInformation("Password complexity rules retrieved successfully.");
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                Data = pwdComplexityRules,
                TotalCount = pwdComplexityRules.TotalCount,
                PageNumber = pwdComplexityRules.PageNumber,
                PageSize = pwdComplexityRules.PageSize
            });

        
        
        }

         [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
           _logger.LogInformation("Starting GetByIdAsync for Password Complexity Rule with ID: {Id}", id);

            // Send the query to get the password complexity rule by ID
        var pwdComplexity = await Mediator.Send(new GetPwdComplexityRuleByIdQuery { Id = id });

     

        _logger.LogInformation("Password Complexity Rule with ID: {Id} retrieved successfully.", id);

        return Ok(new
        {
            StatusCode = StatusCodes.Status200OK,
            Data = pwdComplexity
        });
          

        }
        
          [HttpGet("by-name")]
        public async Task<IActionResult> Getpwdautocomplete([FromQuery] string? name)
        {


            // var companiesClaim = User.FindFirst("companyId")?.Value; 

                _logger.LogInformation("Starting GetAllUserRoleAutoCompleteSearchAsync with search pattern: {SearchPattern}", name);
                    var query = new GetPwdComplexityRuleAutoComplete { SearchTerm  = name ?? string.Empty };
                        var result = await Mediator.Send(query);

                     
                            _logger.LogInformation("Password Complexity Rule found for search pattern: {SearchPattern}. Returning data.", name);

                        return Ok(new
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Data = result
                        });
                     
        
         }

        [HttpPost]
        
        public async Task<IActionResult>CreateAsync([FromBody] CreatePasswordComplexityRuleCommand createPasswordComplexityRuleCommand)
        {
             _logger.LogInformation("Starting CreateAsync for creating a Password Complexity Rule.");

       

        _logger.LogInformation("Validation passed for CreatePasswordComplexityRuleCommand. Proceeding with creation.");

        // Send the command to the Mediator
        var createPasswordComplexityRule = await Mediator.Send(createPasswordComplexityRuleCommand);
      
                _logger.LogInformation("Password Complexity Rule created successfully.");

                return Ok(new
                {
                    StatusCode = StatusCodes.Status201Created,
                    Message = "Password Complexity Rule Created Successfully",
                    Data=createPasswordComplexityRule
                
                });
     
             
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAsync( UpdatePasswordComplexityRuleCommand updatePasswordComplexityRuleCommand)
        {
         
        var PasswordComplexityRule = await Mediator.Send(new GetPwdComplexityRuleByIdQuery { Id = updatePasswordComplexityRuleCommand.Id });
            // Check for ID mismatch
            if (PasswordComplexityRule==null)
            {
                _logger.LogWarning($"Password Complexity Rule ID {updatePasswordComplexityRuleCommand.Id} mismatch." );

                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "PasswordComplexityRule ID mismatch."
                });
            }

            _logger.LogInformation($"Validation passed. Proceeding to update Password Complexity Rule with ID: {updatePasswordComplexityRuleCommand.Id}");

            // Send the update command to the mediator
            var updateResult = await Mediator.Send(updatePasswordComplexityRuleCommand);

            #pragma warning disable CS0472
            if (updateResult == null)
            #pragma warning restore CS0472
            {
                _logger.LogWarning($"Failed to update Password Complexity Rule with ID: {updatePasswordComplexityRuleCommand.Id}");

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = "Failed to update Password Complexity Rule. Please try again later."
                });
            }

            _logger.LogInformation($"Password Complexity Rule with ID: {updatePasswordComplexityRuleCommand.Id} updated successfully.");

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Updated Successfully"
                
            });
        }

        [HttpDelete("{id}")]  
          public async Task<IActionResult> Delete(int  id )
        {
            _logger.LogInformation($"Delete  Password Complexity Rule request started with ID: {id}");

                // Check if the department exists
                var department = await Mediator.Send(new GetPwdComplexityRuleByIdQuery { Id = id });
                if (department == null)
                {
                    _logger.LogWarning($" Password Complexity Rule with ID {id} not found.");

                    return NotFound(new
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = " Password Complexity Rule not found"
                    });
                }

                _logger.LogInformation($" Password Complexity Rule with ID {id} found. Proceeding with deletion.");

                // Attempt to delete the department
                 await Mediator.Send(new DeletePasswordComplexityRuleCommand { Id = id });

              
                    _logger.LogInformation($" Password Complexity Rule with ID {id} deleted successfully.");

                    return Ok(new
                    {
                        Message = " Password Complexity Rule deleted successfully",
                        StatusCode = StatusCodes.Status200OK
                      
                    });
               
        }    

    }

}