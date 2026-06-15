#nullable disable
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Currency.Queries.GetCurrency;
using UserManagement.Application.Currency.Queries.GetCurrencyById;
using UserManagement.Application.Currency.Queries.GetCurrencyAutoComplete;
using UserManagement.Application.Currency.Commands.CreateCurrency;
using UserManagement.Application.Currency.Commands.UpdateCurrency;
using UserManagement.Application.Currency.Commands.DeleteCurrency;
using Microsoft.AspNetCore.Http;

namespace UserManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class CurrencyController : ApiControllerBase
    {
        
        private readonly IMediator _mediator;
        private readonly ILogger<CurrencyController> _logger;

         public CurrencyController(IMediator mediator,
                             ILogger<CurrencyController> logger) 
        : base(mediator)
        {
               
            
            _mediator = mediator; 
            _logger = logger;
        }
        

        [HttpGet]
        public async Task<IActionResult> GetAllCurrencyAsync([FromQuery] int PageNumber,[FromQuery] int PageSize,[FromQuery] string SearchTerm = null)
        {
        
            var result = await Mediator.Send(
            new GetCurrencyQuery
            {
                PageNumber = PageNumber, 
                PageSize = PageSize, 
                SearchTerm = SearchTerm
            });
        if (result is null  || result.Data is null || !result.Data.Any())
        {
            _logger.LogWarning($"No Currency Record {result.Data} not found in DB.");
            return NotFound(new
            {
                message = result.Message,
                statusCode = StatusCodes.Status404NotFound
              
            });
        }
        _logger.LogInformation($"Total {result.Data.Count} active Currencies Listed successfully.");
        return Ok(new
        {
            
            message = result.Message,
            data = result.Data,
            statusCode = StatusCodes.Status200OK,
            TotalCount = result.TotalCount,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize
        });
   
}
    [HttpGet("{id}")]
    [ActionName(nameof(GetByIdAsync))]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        
        if (id <= 0)
        {
            _logger.LogWarning($"CurrencyId {id} not found.");
            return BadRequest(new
            {
                StatusCode = StatusCodes.Status400BadRequest,
                message = "Invalid Entity ID"
            });
        }

        var result = await Mediator.Send(new GetCurrencyByIdQuery { CurrencyId = id });

        if (result == null)
        {
            _logger.LogWarning($"CurrencyId {id} not found.");
            return NotFound(new { StatusCode = StatusCodes.Status404NotFound, message = $"Currency ID {id} not found." });
        }

        return Ok(new
        {
            message = "Currency Listed Successfully",
            statusCode = StatusCodes.Status200OK,
            data = result
        });
        
        
   
}
        [HttpGet("by-name")]
        public async Task<IActionResult> GetCurrency([FromQuery] string CurrencyName)
        {       
        // Fetch entities based on search pattern
        var result = await Mediator.Send(new GetCurrencyAutocompleteQuery { SearchPattern = CurrencyName ?? string.Empty });
        _logger.LogInformation($"Search pattern: {CurrencyName}");
      
         return Ok(new  
            {
                message = "Currency List",
                statusCode = StatusCodes.Status200OK,
                data = result
            });
                     
        }
[HttpPost]
public async Task<IActionResult> CreateAsync(CreateCurrencyCommand createCurrencyCommand)
{
    

    
    var createdcurrencyId = await _mediator.Send(createCurrencyCommand);

     _logger.LogInformation($"Currency {createCurrencyCommand.Code} created successfully.");
      return Ok(new
      {
          StatusCode = StatusCodes.Status201Created,
          message ="Currency Created Successfully",
          data = createdcurrencyId
      });
    
   
  
}
[HttpPut]
public async Task<IActionResult> UpdateAsync( UpdateCurrencyCommand updateCurrencyCommand)
{
  

        await _mediator.Send(updateCurrencyCommand);

            _logger.LogInformation($"Currency {updateCurrencyCommand.Name} updated successfully.");
           return Ok(new
            {
                message = "Currency Updated Successfully",
                statusCode = StatusCodes.Status200OK
            });
       

        
} 

[HttpDelete("{id}")]
public async Task<IActionResult> DeleteCurrencyAsync(int id)
{
        // Process the delete command
         await _mediator.Send(new DeleteCurrencyCommand { Id = id });

       
            _logger.LogInformation($"CurrencyId {id} deleted successfully.");
             return Ok(new
            {
                message = "Currency Deleted Successfully",
                statusCode = StatusCodes.Status200OK
            });
       
   
}



    }
}