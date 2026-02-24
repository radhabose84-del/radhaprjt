using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Infrastructure.Data;
using UserManagement.Application.Common.Interfaces.IFinancialYear;
using UserManagement.Application.FinancialYear.Command.CreateFinancialYear;
using UserManagement.Application.FinancialYear.Command.DeleteFinancialYear;
using UserManagement.Application.FinancialYear.Command.UpdateFinancialYear;
using UserManagement.Application.FinancialYear.Queries.GetFinancialYear;
using UserManagement.Application.FinancialYear.Queries.GetFinancialYearGetById;
using UserManagement.Application.GetFinancialYearYear.Queries.GetFinancialYear;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UserManagement.Application.FinancialYear.Queries.GetFinancialYearAutoComplete;
using Microsoft.AspNetCore.Http;

namespace UserManagement.Presentation.Controllers
{
   [Route("api/[controller]")]
    public class FinancialYearController : ApiControllerBase
    {
      
        
          
          private readonly ILogger<FinancialYearController> _logger;
   

        public FinancialYearController(ISender mediator , ILogger<FinancialYearController> logger ) : base(mediator)
        {
            
            _logger = logger;

        }
        [HttpGet]
        public async Task<IActionResult> GetAllFinancialYearAsync([FromQuery] int PageNumber, [FromQuery] int PageSize, [FromQuery] string? SearchTerm = null)
            {
                _logger.LogInformation("Starting GetAllFinancialYearAsync request.");

                var financialYears = await Mediator.Send(new GetFinancialYearQuery
                {
                    PageNumber = PageNumber,
                    PageSize = PageSize,
                    SearchTerm = SearchTerm
                });

                if (financialYears == null || financialYears.Data == null || !financialYears.Data.Any())
                {
                    _logger.LogWarning("No financial year records found.");
                    return NotFound(new
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "No financial year records found."
                    });
                }

                _logger.LogInformation("Financial year records retrieved successfully.");
                return Ok(new
                {
                    StatusCode = StatusCodes.Status200OK,
                    Data = financialYears.Data,
                    TotalCount = financialYears.TotalCount,
                    PageNumber = financialYears.PageNumber,
                    PageSize = financialYears.PageSize
                });
            }

   

       [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            _logger.LogInformation($"Fetching FinancialYear with ID {id} request started." );
            var financialyr = await Mediator.Send(new GetFinancialYearByIdQuery  { Id = id });
          
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                Data = financialyr
            });
        } 

        [HttpGet("by-Year")]
        public async Task<IActionResult> GetAllFinancialYearAutoCompleteSearchAsync([FromQuery] string? year)
        {
            _logger.LogInformation($"Starting GetAllFinancialYearAutoCompleteSearchAsync with search pattern: {year}");

            var query = new GetFinancialYearAutoCompleteQuery { SearchTerm = year ?? string.Empty };
            var result = await Mediator.Send(query);

          
                _logger.LogInformation($"Financial years found for search pattern: {year}. Returning data.");

                return Ok(new
                {
                    StatusCode = StatusCodes.Status200OK,
                    Data = result
                });
           
        }

       

        [HttpPost]
         
        public async Task<IActionResult>CreateAsync([FromBody] CreateFinancialYearCommand command)
        {
                _logger.LogInformation($"Create Financial Year request started with data: {command}");

          

            // Process the command
            var createFinancialYear = await Mediator.Send(command);
        
                return Ok(new
                {
                    StatusCode = StatusCodes.Status201Created,
                    Message = "Financial Year created successfully",
                    Data = createFinancialYear
                });
          
            
               
        }

        [HttpPut]   
        public async Task<IActionResult> UpdateAsync([FromBody] UpdateFinancialYearCommand command)
        {
                    if (command == null)
            {
                _logger.LogError("UpdateFinancialYearCommand is null.");
                return BadRequest("Invalid request: UpdateFinancialYearCommand is required.");
            }
             _logger.LogInformation($"Update Financial Year request started with data: {command}" );

           
             var financialyearresult = await Mediator.Send(new GetFinancialYearByIdQuery { Id = command.Id });
            if (financialyearresult == null)
            {
                _logger.LogWarning($"Financial Year with ID {command.Id} not found.");

                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Financial Year not found"
                });
            }


         

                      if (command == null)
            {
                _logger.LogError("Command is null before sending to Mediator.");
                return BadRequest("Command is null before sending to Mediator.");
            }
            // Update the department
             await Mediator.Send(command);
       
                _logger.LogInformation($"Financial Year  with ID {command.Id} updated successfully." );

                return Ok(new
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Financial Year  updated successfully"
                  
                });
         
        

            }


             [HttpDelete("{id}")]   
             
            public async Task<IActionResult> Delete(int id )
            {
            _logger.LogInformation($"Delete FinancialYear Command request started with ID: {id}");

                // Check if the department exists
                var department = await Mediator.Send(new GetFinancialYearByIdQuery { Id = id });
                if (department == null)
                {
                    _logger.LogWarning($"FinancialYear  with ID {id} not found." );

                    return NotFound(new
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "FinancialYear not found"
                    });
                }

                _logger.LogInformation($"FinancialYear with ID {id} found. Proceeding with deletion.");

                // Attempt to delete the department
                 await Mediator.Send( new DeleteFinancialYearCommand { Id=id} );

                    _logger.LogInformation($"FinancialYear with ID {id} deleted successfully." );

                    return Ok(new
                    {
                        Message = "FinancialYear deleted successfully",
                        StatusCode = StatusCodes.Status200OK
                      
                    });
             


     
        }

      


    }

}