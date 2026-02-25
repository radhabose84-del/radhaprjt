#nullable disable
using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.Units.Queries.GetUnits;
using UserManagement.Application.Units.Queries.GetUnitById;
using UserManagement.Application.Units.Commands.CreateUnit;
using UserManagement.Application.Units.Commands.DeleteUnit;
using UserManagement.Application.Units.Commands.UpdateUnit;
using UserManagement.Application.Units.Queries.GetUnitAutoComplete;
using UserManagement.Application.Units.Queries.GetUnitByUserId;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace UserManagement.Presentation.Controllers
{
    [Route("api/[controller]")]

    public class UnitController : ApiControllerBase
    {
       
        private readonly ILogger<UnitController> _logger;
        public UnitController(ISender mediator,
        ILogger<UnitController> logger) 
        : base(mediator)
        {
            
            _logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllUnitsAsync([FromQuery] int PageNumber,[FromQuery] int PageSize,[FromQuery] string SearchTerm = null)
        {
            var result = await Mediator.Send(
            new GetUnitQuery
             {
                PageNumber = PageNumber, 
                PageSize = PageSize, 
                SearchTerm = SearchTerm
                
            });
            
        if (result is null || result.Data is null || !result.Data.Any())
        {
            _logger.LogInformation($"No Unit Record {result.Data} not found in DB.");
            return NotFound(new
            {
                message = result.Message,
                statusCode = StatusCodes.Status404NotFound
                
            });
        }
         
        _logger.LogInformation($"Unit {result.Data.Count} Active Listed successfully.");
        return Ok(new
        {
            message = "Units retrieved successfully.",
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
            _logger.LogWarning($"UnitId {id} not found.");
            
            return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message ="Invalid Unit ID"
                });
        }
            var unit = await Mediator.Send(new GetUnitByIdQuery() { Id = id});
        
                return Ok(new
                {
                    message = "Unit Fetched Successfully",
                    statusCode = StatusCodes.Status200OK,
                    data = unit
                });
         
        
        }

    [HttpPost]
    public async Task<IActionResult> CreateUnitAsync(CreateUnitCommand createUnitCommand)
    {
     
        var createdUnit = await Mediator.Send(createUnitCommand);
        
             return Ok(new
             {
                 message = "Unit Created Successfully",
                 statusCode = StatusCodes.Status201Created,
                 data = createdUnit
             });
     
       
    }


    [HttpPut("update")]
    public async Task<IActionResult> UpdateUnitAsync( UpdateUnitCommand updateUnitCommand)
    {
       
       
        var result = await Mediator.Send(updateUnitCommand);
    
            return Ok(new
            {
                message = "Unit Updated Successfully",
                statusCode = StatusCodes.Status200OK
             
            });
    
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUnitAsync(int id)
    {
        
       var result = await Mediator.Send(new DeleteUnitCommand { UnitId = id });

      
            _logger.LogInformation($"UnitId {id} deleted successfully.");
            return Ok(new
            {
                message = "Unit Deleted Successfully",
                statusCode = StatusCodes.Status200OK,
                
            });
        
    }

       [HttpGet("by-name")]
        public async Task<IActionResult> GetUnit([FromQuery] string unitname,int? CompanyId)
        {
            var units = await Mediator.Send(new GetUnitAutoCompleteQuery {SearchPattern = unitname??string.Empty, CompanyId = CompanyId??0});
             _logger.LogInformation("Search pattern: {SearchPattern}", unitname);
           
             
                return Ok(new
                {
                    message = "Unit List",
                    statusCode = StatusCodes.Status200OK,
                    data = units
                });
          
          
        }
      [HttpGet("by-userid")]
        public async Task<IActionResult> GetUnitByUserId([FromQuery] int? CompanyId,int UserId)
        {
            var units = await Mediator.Send(new GetUnitByUserIdQuery { CompanyId = CompanyId??0 ,UserId=UserId});
             
           
                return Ok(new
                {
                    message = "Unit List",
                    statusCode = StatusCodes.Status200OK,
                    data = units
                });
         
          
        }
    }
}