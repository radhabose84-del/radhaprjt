

using FAM.Application.WDVDepreciation.Commands.CreateDepreciation;
using FAM.Application.WDVDepreciation.Commands.DeleteDepreciation;
using FAM.Application.WDVDepreciation.Commands.LockDepreciation;
using FAM.Application.WDVDepreciation.Queries.GetDepreciation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FAM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WDVDepreciationController  : ApiControllerBase
    {
         public WDVDepreciationController(ISender mediator) 
        : base(mediator) { }    
        [HttpGet("GetWDV")]
        public async Task<IActionResult> GetDepreciationAsync([FromQuery] GetDepreciationQuery request)
        {
            var data = await Mediator.Send(request);
            return Ok(data);
        }
        
        [HttpPost]               
        public async Task<IActionResult> CreateAsync(CreateDepreciationCommand  command)
        {             
            var result = await Mediator.Send(command);
        
                return Ok(new 
                { 
                    StatusCode=StatusCodes.Status201Created,
                    message = "WDV Depreciation Details Created", 
                    data = result
                });
          
        }        
        [HttpDelete]        
        public async Task<IActionResult> DeleteAsync(DeleteDepreciationCommand  command)
        {             
            await Mediator.Send(command);
          
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data =$"WDV Depreciation Details Deleted" ,
                message = "Depreciation Details Deleted"
            });
        }
         [HttpPut]        
        public async Task<IActionResult> LockDepreciationAsync(LockDepreciationCommand  command)
        {             
             await Mediator.Send(command);
         
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data =$"Depreciation Details Locked" ,
                message = "Depreciation Details Locked"
            });
        }                       
    }
}