using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Application.SwitchProfile.Commands.SwitchProfileByUnit;
using UserManagement.Application.SwitchProfile.Queries.GetUnitProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace UserManagement.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SwitchProfileController : ApiControllerBase
    {
        public SwitchProfileController(ISender mediator)
        : base(mediator)
        {
            
        }
         [HttpGet("by-name")]
        public async Task<IActionResult> GetUnit()
        {
            var units = await Mediator.Send(new GetUnitProfileQuery {});
             
           
                return Ok(new
                {
                    message = "Unit List",
                    statusCode = StatusCodes.Status200OK,
                    data = units
                });
          
          
        }
           [HttpPost("SwitchProfile")]   
        public async Task<IActionResult> SwitchProfile(SwitchProfileByUnitCommand  command)
        { 
                          
            var result = await Mediator.Send(command);
                          
                return Ok(new { StatusCode=StatusCodes.Status201Created, message = "Switched Profile", errors = "", data = result });
           
            
        }
    }
}