using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using WarehouseManagement.Application.Common.Interfaces.IRackMaster;
using WarehouseManagement.Application.RackMaster.Command.CreateRackMaster;
using WarehouseManagement.Application.RackMaster.Command.DeleteRackMaster;
using WarehouseManagement.Application.RackMaster.Command.UpdateRackMaster;
using WarehouseManagement.Application.RackMaster.Queries.GetAllRackMaster;
using WarehouseManagement.Application.RackMaster.Queries.GetRackMasterAutoComplete;
using WarehouseManagement.Application.RackMaster.Queries.GetRackMasterById;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace WarehouseManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class RackMasterController : ApiControllerBase
    {
        private readonly IRackMasterQueryRepository _rackMasterQueryRepository;

        public RackMasterController(ISender mediator, IRackMasterQueryRepository rackMasterQueryRepository) : base(mediator)
        {
            _rackMasterQueryRepository = rackMasterQueryRepository;

        }

        [HttpGet]
        public async Task<IActionResult> GetAllWarehouseMasterAsync([FromQuery] int PageNumber, [FromQuery] int PageSize, [FromQuery] string? SearchTerm = null)
        {
            var rackmaster = await Mediator.Send(
                new GetAllRackMasterQuery
                {
                    PageNumber = PageNumber,
                    PageSize = PageSize,
                    SearchTerm = SearchTerm
                });
            // var activecompanies = companies.Data.ToList(); 

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = rackmaster.Data,
                TotalCount = rackmaster.TotalCount,
                PageNumber = rackmaster.PageNumber,
                PageSize = rackmaster.PageSize
            });
        }

        [HttpGet("{id}")]
        [ActionName(nameof(GetByIdAsync))]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var rackmaster = await Mediator.Send(new GetRackMasterByIdQuery { Id = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                IsSuccess = true,
                Message = "Rack master fetched successfully.",
                Data = rackmaster
            });
        }


        // public async Task<IActionResult> CreateAsync([FromBody] CreateRackMasterCommand cmd, CancellationToken ct = default)
        // {
        //     var newId = await Mediator.Send(cmd, ct);

        //     return CreatedAtAction(
        //         nameof(GetByIdAsync),
        //         new { id = newId },
        //         new ApiResponseDTO<int>
        //         {
        //             StatusCode = StatusCodes.Status201Created,
        //             IsSuccess  = true,
        //             Message    = "Rack master created.",
        //             Data       = newId
        //         });
        // }

        [HttpPost("create")]
        public async Task<IActionResult> CreateAsync(CreateRackMasterCommand command)
        {


            var response = await Mediator.Send(command);

            return Ok(new
            {
                StatusCode = StatusCodes.Status201Created,
                message = "Created Successfully",
                errors = "",
                data = response
            });

        }

        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] UpdateRackMasterCommand command)
        {
            var result = await Mediator.Send(command);

            if (result == 0)
            {
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Update Failed",
                    errors = result

                });
            }

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Updated Successfully",
                data = result
            });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteWarehouseAsync(int id, CancellationToken ct)
        {
            await Mediator.Send(new DeleteRackMasterCommand { Id = id }, ct);

            return Ok(new
            {
                message = "Deleted successfully.",
                statusCode = StatusCodes.Status200OK
            });
        }
        

        [HttpGet("by-name")]
        public async Task<IActionResult> GetRackMaster([FromQuery] string? name = null , [FromQuery] int warehouseId = 0)
        {
            var result = await Mediator.Send(new GetRackMasterAutoCompleteQuery { SearchPattern = name , WarehouseId = warehouseId });
            if (result == null)
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    message = "  Rack Not Found",
                });
            }
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = $"Rack Found",
                data = result
            });
        }
        
    }
}