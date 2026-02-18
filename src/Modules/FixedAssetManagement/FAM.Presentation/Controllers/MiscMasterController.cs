using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using FAM.Application.Common.Interfaces.IMiscMaster;
using FAM.Application.ExcelImport.MiscMaster;
using FAM.Application.MiscMaster.Command.CreateMiscMaster;
using FAM.Application.MiscMaster.Command.DeleteMiscMaster;
using FAM.Application.MiscMaster.Command.UpdateMiscMaster;
using FAM.Application.MiscMaster.Queries.GetMiscMaster;
using FAM.Application.MiscMaster.Queries.GetMiscMasterAutoComplete;
using FAM.Application.MiscMaster.Queries.GetMiscMasterById;
using FAM.Domain.Entities;
using FAM.Infrastructure.Repositories.MiscMaster;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FAM.Presentation.Controllers
{
    [ApiController]
    [Route("api/fam/[controller]")]
    public class MiscMasterController : ApiControllerBase
    {
        private readonly IMiscMasterCommandRepository _miscMasterCommandRepository;
        public MiscMasterController(ISender mediator,IMiscMasterCommandRepository miscMasterCommandRepository) 
        : base(mediator)
        {

            _miscMasterCommandRepository = miscMasterCommandRepository;

        }

        [HttpGet]
        public async Task<IActionResult> GetAllMiscMasterAsync([FromQuery] int PageNumber, [FromQuery] int PageSize, [FromQuery] string? SearchTerm = null)
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
                StatusCode = StatusCodes.Status200OK,
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

            var miscmaster = await Mediator.Send(new GetMiscMasterByIdQuery() { Id = id });

          
                return Ok(new { StatusCode = StatusCodes.Status200OK, data = miscmaster, message = miscmaster });
           

        }
        [HttpGet("by-name")]
        public async Task<IActionResult> GetMiscMaster([FromQuery] string? name, [FromQuery] string MiscTypeCode)
        {

            var miscmaster = await Mediator.Send(new GetMiscMasterAutoCompleteQuery { MiscTypeName = name, MiscTypeCode = MiscTypeCode });
          
                return Ok(new { StatusCode = StatusCodes.Status200OK, data = miscmaster });
          
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateMiscMasterCommand command)
        {
            
            var response = await Mediator.Send(command);
         
                return Ok(new
                {
                    StatusCode = StatusCodes.Status201Created,
                    message = "MiscMaster created successfully",
                    errors = "",
                    data = response
                });
          

        }
        [HttpPut]

        public async Task<IActionResult> Update(UpdateMiscMasterCommand command)
        {



            // Check if the record exists
            var miscMasterExists = await Mediator.Send(new GetMiscMasterByIdQuery { Id = command.Id });
            if (miscMasterExists == null)
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = $"MiscMaster ID {command.Id} not found.",
                    Errors = ""
                });
            }

            // Check if SortOrder is not greater than the max value in the table
            var maxSortOrder = await _miscMasterCommandRepository.GetMaxSortOrderAsync();
            if (command.SortOrder > maxSortOrder)
            {
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = $"SortOrder cannot be greater than the maximum value ({maxSortOrder}) in the table.",
                    Errors = ""
                });
            }

            // Update the record
             await Mediator.Send(command);
        
                return Ok(new
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "MiscMaster updated successfully",
                    Errors = ""
                });
         
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {

             await Mediator.Send(new DeleteMiscMasterCommand { Id = id });

           
                return Ok(new { StatusCode = StatusCodes.Status200OK, message = "MiscMaster deleted successfully", errors = "" });

    
        }

        [HttpPost("import")]
        [Consumes("multipart/form-data")]
      public async Task<IActionResult> Import([FromForm] FAM.Application.ExcelImport.MiscMaster.MiscMasterImportRequest request)
        {
            if (request.File == null || request.File.Length == 0)
                return BadRequest(new ApiResponseDTO<bool>
                {
                    IsSuccess = false,
                    Message = "No file uploaded.",
                    Data = false
                });

            var command = new MiscMasterImportCommand(request.File);
            var result = await Mediator.Send(command);

            if (result.IsSuccess)
            {
                return Ok(new
                {
                    status = 200,
                    success = true,
                    message = result.Message
                });
            }
            else
            {
                return BadRequest(new
                {
                    status = 400,
                    success = false,
                    message = result.Message
                });
            }
        }

        
      
    }
}