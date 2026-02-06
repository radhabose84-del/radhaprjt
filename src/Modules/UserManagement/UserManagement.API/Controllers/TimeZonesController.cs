using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UserManagement.Application.TimeZones.Queries.GetTimeZones;
using UserManagement.Application.TimeZones.Queries.GetTimeZonesAutoComplete;
using UserManagement.Application.TimeZones.Queries.GetTimeZonesById;
using UserManagement.Infrastructure.Data;
using MediatR;
using Polly.Caching;

namespace UserManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TimeZonesController : ApiControllerBase
    {
        private readonly ILogger<TimeZonesController> _logger;
        private readonly IMediator _mediator;

        public TimeZonesController(ILogger<TimeZonesController> logger, IMediator mediator)
         : base(mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllTimeZonesAsync([FromQuery] int PageNumber,[FromQuery] int PageSize,[FromQuery] string? SearchTerm = null)
        {
        
        var result  = await Mediator.Send(new 
        GetTimeZonesQuery
         {
                PageNumber = PageNumber, 
                PageSize = PageSize, 
                SearchTerm = SearchTerm
            });
        if (result is null || result.Data is null || !result.Data.Any())
        {
            _logger.LogWarning($"No TimeZone Record {result.Data} not found in DB.");
            return NotFound(new
            {
                message = result.Message,
                statusCode = StatusCodes.Status404NotFound
              
            });
        }
        _logger.LogInformation($"TimeZone {result.Data.Count} Active Listed successfully.");
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
            _logger.LogWarning($"TimeZoneId {id} not found.");
            return BadRequest(new
            {
                StatusCode = StatusCodes.Status400BadRequest,
                message = "Invalid TimeZone ID"
            });
        }

        var result = await Mediator.Send(new GetTimeZoneByIdQuery { TimeZoneId = id });

     
              return Ok(new
             {
                 message = "TimeZone Fetched Successfully",
                 statusCode = StatusCodes.Status200OK,
                 data = result
             }); 
       
   
}
        [HttpGet("by-name")]
        public async Task<IActionResult> GetTimeZones([FromQuery] string? TimeZoneName)
        {       
        // Fetch entities based on search pattern
        var result = await Mediator.Send(new GetTimeZonesAutocompleteQuery { SearchPattern = TimeZoneName?? string.Empty });
       _logger.LogInformation($"Search pattern {TimeZoneName} cannot be empty.");
    
         return Ok(new  
            {
                message = "TimeZone List",
                statusCode = StatusCodes.Status200OK,
                data = result
            });
                     
}
   
    }
}