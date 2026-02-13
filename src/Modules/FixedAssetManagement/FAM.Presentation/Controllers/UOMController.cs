using MediatR;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using FAM.Application.UOM.Command.CreateUOM;
using FAM.Application.UOM.Command.UpdateUOM;
using FAM.Application.UOM.Queries.GetUOMs;
using FAM.Application.UOM.Queries.GetUOMById;
using FAM.Application.UOM.Command.DeleteUOM;
using FAM.Application.UOM.Queries.GetUOMAutoComplete;
using FAM.Application.UOM.Queries.GetUOMTypeAutoComplete;
using Microsoft.AspNetCore.Http;

namespace FAM.Presentation.Controllers
{
    [ApiController]
    [Route("api/fam/[controller]")]
    
    public class UOMController : ApiControllerBase
    {

        public UOMController(ISender mediator
        ) 
        : base(mediator)
        {
            
        }
    [HttpGet]
    public async Task<IActionResult> GetAllUOMAsync([FromQuery] int PageNumber,[FromQuery] int PageSize,[FromQuery] string? SearchTerm = null)
    {
        var uom = await Mediator.Send(
            new GetUOMQuery
            {
                PageNumber = PageNumber, 
                PageSize = PageSize, 
                SearchTerm = SearchTerm
            });
           
            return Ok( new 
            { 
                StatusCode=StatusCodes.Status200OK, 
                data = uom.Data.ToList(),
                TotalCount = uom.TotalCount,
                PageNumber = uom.PageNumber,
                PageSize = uom.PageSize
                });
    }
    [HttpPost]
    public async Task<IActionResult> CreateAsync(CreateUOMCommand createuomcommand)
    {
            
   
        var result = await Mediator.Send(createuomcommand);

                return Ok(new 
                { 
                    StatusCode=StatusCodes.Status201Created,
                    message = "UOM created successfully", 
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
                    message = "Invalid UOM ID"
                });
            }  
       var result = await Mediator.Send(new GetUOMByIdQuery() { Id = id});
          
                    return Ok(new
                    {
                        StatusCode = StatusCodes.Status200OK,
                        data = result
                    });
    }

    [HttpPut]
    public async Task<IActionResult> Update( UpdateUOMCommand updateUomcommand )
    {
        

        var uomExists = await Mediator.Send(new GetUOMByIdQuery { Id = updateUomcommand.Id });

             if (uomExists == null)
             {
                 return NotFound(new { StatusCode=StatusCodes.Status404NotFound, message = $"UOM ID {updateUomcommand.Id} not found.", errors = "" }); 
             }

        var result = await Mediator.Send(updateUomcommand);
        
                return Ok(new 
                { 
                    StatusCode=StatusCodes.Status201Created,
                    message = "UOM updated successfully", 
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
                    message = "Invalid UOM ID"
                });
            } 
         await Mediator.Send(new DeleteUOMCommand { Id = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data =$"UOM ID {id} Deleted" 
            });
            
    }

    [HttpGet("by-name")]
    public async Task<IActionResult> GetUOM([FromQuery] string? name)
    {
        var result = await Mediator.Send(new GetUOMAutoCompleteQuery {SearchPattern = name});
      
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = result,
                data = result
            });
    }
     [HttpGet("by-Type")]
    public async Task<IActionResult> GetUOMType([FromQuery] string? name)
    {
        var result = await Mediator.Send(new GetUOMTypeAutoCompleteQuery {SearchPattern = name});
      
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = result,
                data = result
            });
    }
    }
}