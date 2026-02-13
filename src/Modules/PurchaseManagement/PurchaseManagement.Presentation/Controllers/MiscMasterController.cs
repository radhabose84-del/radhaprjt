using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.MiscMaster.Command.CreateMiscMaster;
using PurchaseManagement.Application.MiscMaster.Command.DeleteMiscMaster;
using PurchaseManagement.Application.MiscMaster.Command.UpdateMiscMaster;
using PurchaseManagement.Application.MiscMaster.Queries.GetMiscMaster;
using PurchaseManagement.Application.MiscMaster.Queries.GetMiscMasterAutoComplete;
using PurchaseManagement.Application.MiscMaster.Queries.GetMiscMasterById;
using PurchaseManagement.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace PurchaseManagement.Presentation.Controllers
{
    [ApiController]
     [Route("api/purchase/[controller]")]

    public class MiscMasterController  : ApiControllerBase
    {
        private readonly ILogger<MiscMaster> _logger;
        private  readonly IValidator<CreateMiscMasterCommand> _miscMasterCommand;
        private readonly IValidator<UpdateMiscMasterCommand> _updateMiscMasterCommand;

        private readonly IValidator<DeleteMiscMasterCommand> _deleteMiscMasterCommand;
        private readonly IMiscMasterCommandRepository _miscMasterCommandRepository;
        
        
       public MiscMasterController(ISender mediator, IValidator<CreateMiscMasterCommand> miscMasterCommand, IValidator<UpdateMiscMasterCommand> updateMiscMasterCommand, IMiscMasterCommandRepository miscMasterCommandRepository,
          IValidator<DeleteMiscMasterCommand> deleteMiscMasterCommand) : base(mediator)
        {

            _miscMasterCommand = miscMasterCommand;
            _updateMiscMasterCommand = updateMiscMasterCommand;
            _miscMasterCommandRepository = miscMasterCommandRepository;
            _deleteMiscMasterCommand = deleteMiscMasterCommand;
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

            var validationResult = await _miscMasterCommand.ValidateAsync(command);

            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = "Validation failed",
                    errors = validationResult.Errors.Select(e => e.ErrorMessage).ToArray()
                });
            }
           
            var response = await Mediator.Send(command);    
                                                                             
                return Ok(new
            {
                StatusCode = StatusCodes.Status201Created,
                message = "Created Successfully",
                errors = "",
                data = response
            });
            
        } 
        [HttpPut] 
         public async Task<IActionResult> Update(UpdateMiscMasterCommand command)
        {
              var validationResult = await _updateMiscMasterCommand.ValidateAsync(command);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }          


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