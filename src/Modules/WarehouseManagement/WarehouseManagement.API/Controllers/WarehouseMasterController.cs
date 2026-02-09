using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WarehouseManagement.Application.Common.Interfaces.IWarehouseMaster;
using WarehouseManagement.Application.WarehouseMaster;
using WarehouseManagement.Application.WarehouseMaster.Command.CreateWarehouseMaster;
using WarehouseManagement.Application.WarehouseMaster.Command.DeleteWarehouseMaster;
using WarehouseManagement.Application.WarehouseMaster.Command.UpdateWarehouseMaster;
using WarehouseManagement.Application.WarehouseMaster.GetWarehouseMasterById;
using WarehouseManagement.Application.WarehouseMaster.Queries.GetParentWarehouseMaster;
using WarehouseManagement.Application.WarehouseMaster.Queries.GetWareMasterAutoComplete;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace WarehouseManagement.API.Controllers
{
    [Route("api/[controller]")]
    public class WarehouseMasterController : ApiControllerBase
    {
        private readonly IWarehouseMasterQueryRepository _warehouseMasterQueryRepository;

        public WarehouseMasterController(ISender mediator, IWarehouseMasterQueryRepository warehouseMaster) : base(mediator)
        {
            _warehouseMasterQueryRepository = warehouseMaster;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllWarehouseMasterAsync([FromQuery] int PageNumber, [FromQuery] int PageSize, [FromQuery] string? SearchTerm = null)
        {

            var warehousemaster = await Mediator.Send(
                new GetAllWarehouseMastersQuery
                {
                    PageNumber = PageNumber,
                    PageSize = PageSize,
                    SearchTerm = SearchTerm
                });
            // var activecompanies = companies.Data.ToList(); 

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = warehousemaster.Data,
                TotalCount = warehousemaster.TotalCount,
                PageNumber = warehousemaster.PageNumber,
                PageSize = warehousemaster.PageSize
            });
        }

        [HttpGet("{id}")]
        [ActionName(nameof(GetByIdAsync))]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var warehouseMaster = await Mediator.Send(new GetWarehouseMasterByIdQuery { Id = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = warehouseMaster.Data, // Only the DTO
                message = warehouseMaster.Message
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateWarehouseMasterCommand createWarehouseMasterCommand)
        {
            // Send the command to the handler via MediatR
            var createdWarehouseId = await Mediator.Send(createWarehouseMasterCommand);

            return Ok(new
            {
                StatusCode = StatusCodes.Status201Created,
                message = "Warehouse created successfully.",
                data = createdWarehouseId
            });
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] UpdateWarehouseMasterCommand command)
        {
            var result = await Mediator.Send(command);

            if (!result.IsSuccess)
            {
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = result.Message,
                    errors = result.Errors
                });
            }

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                Message = result.Message,
                errors = ""
            });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteWarehouseAsync(int id, CancellationToken ct)
        {
            await Mediator.Send(new DeleteWarehouseMasterCommand { Id = id }, ct);

            return Ok(new
            {
                message = "Deleted successfully.",
                statusCode = StatusCodes.Status200OK
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetRackMaster([FromQuery] string? name)
        {
            var result = await Mediator.Send(new GetWarehouseMasterAutoCompleteQuery { SearchPattern = name });
            if (result == null)
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    message = "  Warehouse Not Found",
                });
            }
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }
        

         [HttpGet("Get Parent Warehouse")]
        public async Task<IActionResult> GetParentWarehouse([FromQuery] string? name)
        {
            var result = await Mediator.Send(new GetParentWarehouseMasterQuery {} );
            if (result == null)
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    message = "  Parent Warehouse Not Found",
                });
            }
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,                
                data = result
            });
        }

      
    }
}