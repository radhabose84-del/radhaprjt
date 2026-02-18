#nullable disable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.Location.Command.DeleteAubLocation;
using FAM.Application.Location.Command.UpdateSubLocation;
using FAM.Application.SubLocation.Command.CreateSubLocation;
using FAM.Application.SubLocation.Queries.GetSubLocationAutoComplete;
using FAM.Application.SubLocation.Queries.GetSubLocationById;
using FAM.Application.SubLocation.Queries.GetSubLocations;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FAM.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubLocationController : ApiControllerBase
    {
        

    public SubLocationController(ISender mediator
    ) 
        : base(mediator)
    {
           
    }
    [HttpGet]
    public async Task<IActionResult> GetAllSubLocationAsync([FromQuery] int PageNumber,[FromQuery] int PageSize,[FromQuery] string SearchTerm = null)
    {
        var sublocations = await Mediator.Send(
            new GetSubLocationQuery
            {
                PageNumber = PageNumber, 
                PageSize = PageSize, 
                SearchTerm = SearchTerm
            });
           
            return Ok( new 
            { 
                StatusCode=StatusCodes.Status200OK, 
                data = sublocations.Data.ToList(),
                TotalCount = sublocations.TotalCount,
                PageNumber = sublocations.PageNumber,
                PageSize = sublocations.PageSize
                });
        }
    [HttpPost]
    public async Task<IActionResult> CreateAsync(CreateSubLocationCommand createsublocationcommand)
    {
            
       
            var result = await Mediator.Send(createsublocationcommand);
        
                return Ok(new 
                { 
                    StatusCode=StatusCodes.Status201Created,
                    message = "SubLocation Created Successfully", 
                    data = result
                });
         
            
        }
    [HttpGet("{id}")]
    [ActionName(nameof(GetByIdAsync))]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
         if (id <= 0)
            {
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = "Invalid SubLocation ID"
                });
            }  
           
        var result = await Mediator.Send(new GetSubLocationByIdQuery() { Id = id});
          
      
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }
    [HttpPut]
    public async Task<IActionResult> Update( UpdateSubLocationCommand updatesubLocationcommand )
    {
       
       var locationExists = await Mediator.Send(new GetSubLocationByIdQuery { Id = updatesubLocationcommand.Id });

        if (locationExists == null)
        {
            return NotFound(new { StatusCode=StatusCodes.Status404NotFound, message = $"SubLocation ID {updatesubLocationcommand.Id} not found.", errors = "" }); 
        }

        var result = await Mediator.Send(updatesubLocationcommand);
       
                return Ok(new 
                { 
                    StatusCode=StatusCodes.Status201Created,
                    message = "SubLocation Updated Successfully", 
                    data = result 
                });
          
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
         if (id <= 0)
            {
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = "Invalid SubLocation ID"
                });
            } 
           
        var deletedsublocation = await Mediator.Send(new DeleteSubLocationCommand { Id = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data =$"SubLocation ID {id} Deleted",
                message = "SubLocation Deleted Successfully" 
            });
    }
    [HttpGet("by-name")]
    public async Task<IActionResult> GetSubLocation([FromQuery] string name)
    {
        var result = await Mediator.Send(new GetSubLocationAutoCompleteQuery {SearchPattern = name});
      
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = result,
                data = result
            });
    }

    }
}