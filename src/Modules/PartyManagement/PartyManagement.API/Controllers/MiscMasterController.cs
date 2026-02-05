using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using PartyManagement.Application.MiscMaster.Command.CreateMiscMaster;
using PartyManagement.Application.MiscMaster.Command.DeleteMiscMaster;
using PartyManagement.Application.MiscMaster.Command.UpdateMiscMaster;
using PartyManagement.Application.MiscMaster.Queries.GetMiscMaster;
using PartyManagement.Application.MiscMaster.Queries.GetMiscMasterAutoComplete;
using PartyManagement.Application.MiscMaster.Queries.GetMiscMasterById;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace PartyManagement.API.Controllers
{
    [Route("api/[controller]")]
    public class MiscMasterController : ApiControllerBase
    {
        public MiscMasterController(ISender mediator) : base(mediator)
        {

        }
        [HttpGet] 
          public async Task<IActionResult> GetAllMiscMasterAsync([FromQuery] int PageNumber,[FromQuery] int PageSize,[FromQuery] string? SearchTerm = null)
        {
            var miscmaster = await Mediator.Send(
            new GetMiscMasterQuery
            {
                PageNumber = PageNumber, 
                PageSize = PageSize, 
                SearchTerm = SearchTerm
            });
           // var activecompanies = companies.Data.ToList(); 

            return Ok(new 
            { 
                StatusCode=StatusCodes.Status200OK, 
                data = miscmaster.Data,
                TotalCount = miscmaster.TotalCount,
                PageNumber = miscmaster.PageNumber,
                PageSize = miscmaster.PageSize
            });
        }
        [HttpGet("{id}")]
        [ActionName(nameof(GetByIdAsync))]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
           
            var miscmaster = await Mediator.Send(new GetMiscMasterByIdQuery() { Id = id});
          
            return Ok(new { StatusCode=StatusCodes.Status200OK, data = miscmaster,message=miscmaster});
        
           
        }
            [HttpGet("by-name")]
        public async Task<IActionResult> GetMiscMaster([FromQuery] string? name,[FromQuery] string MiscTypeCode)
        {
          
            var miscmaster = await Mediator.Send(new GetMiscMasterAutoCompleteQuery {SearchPattern = name,MiscTypeCode=MiscTypeCode});
            
            return Ok( new { StatusCode=StatusCodes.Status200OK, data = miscmaster });
            
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateMiscMasterCommand command)
        {
           
            var response = await Mediator.Send(command);
                                      
                return Ok(new 
                {
                     StatusCode=StatusCodes.Status201Created,
                 message = "Created Successfully",
                  errors = "",
                  data = response 
                  });
            
            
        } 
        [HttpPut] 
         public async Task<IActionResult> Update(UpdateMiscMasterCommand command)
        {
           

             await Mediator.Send(command);
            
                return Ok(new 
                { 
                    StatusCode = StatusCodes.Status200OK, 
                    Message = "Updated Successfully", 
                    Errors = "" 
                });
            

        }
        [ HttpDelete("{id}")]
          public async Task<IActionResult> Delete(int id)
        {
        
           
            await Mediator.Send(new DeleteMiscMasterCommand { Id = id });

            return Ok(new { StatusCode=StatusCodes.Status200OK, message = "Deleted successfully.", errors = "" });
              
            
        }  

     
    }
}