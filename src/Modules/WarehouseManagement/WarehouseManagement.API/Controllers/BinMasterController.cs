using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WarehouseManagement.Application.BinMaster.Command.CreateBinMaster;
using WarehouseManagement.Application.BinMaster.Command.DeleteBinMaster;
using WarehouseManagement.Application.BinMaster.Command.UpdateBinMaster;
using WarehouseManagement.Application.BinMaster.Queries.GetAllBinMaster;
using WarehouseManagement.Application.BinMaster.Queries.GetBinMasterAutoComplete;
using WarehouseManagement.Application.BinMaster.Queries.GetBinMasterById;
using WarehouseManagement.Application.Common.Interfaces.IBinMaster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace WarehouseManagement.API.Controllers
{
    [Route("[controller]")]
    public class BinMasterController : ApiControllerBase
    {

        private readonly IBinMasterQueryRepository _binMasterQueryRepository;
        public BinMasterController(ISender mediator, IBinMasterQueryRepository binMasterQueryRepository) : base(mediator)
        {
            _binMasterQueryRepository = binMasterQueryRepository;

        }

        [HttpGet]
        public async Task<IActionResult> GetAllBinMasterAsync([FromQuery] int PageNumber, [FromQuery] int PageSize, [FromQuery] string? SearchTerm = null)
        {

            var binmaster = await Mediator.Send(
                new GetAllBinMasterQuery
                {
                    PageNumber = PageNumber,
                    PageSize = PageSize,
                    SearchTerm = SearchTerm
                });
            // var activecompanies = companies.Data.ToList(); 

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = binmaster.Data,
                TotalCount = binmaster.TotalCount,
                PageNumber = binmaster.PageNumber,
                PageSize = binmaster.PageSize
            });
        }

        [HttpGet("{id}")]
        [ActionName(nameof(GetByIdAsync))]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var rackmaster = await Mediator.Send(new GetBinMasterByIdQuery { Id = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                IsSuccess = true,
                Message = "Bin master fetched successfully.",
                Data = rackmaster
            });
        }


        [HttpPost("create")]
        public async Task<IActionResult> CreateAsync(CreateBinMasterCommand command)
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
        public async Task<IActionResult> Update([FromBody] UpdateBinMasterCommand command)
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
            await Mediator.Send(new DeleteBinMasterCommand { Id = id }, ct);

            return Ok(new
            {
                message = "Deleted successfully.",
                statusCode = StatusCodes.Status200OK
            });
        }
        
        
        [HttpGet("by-name")]
       public async Task<IActionResult> GetBinMaster(  [FromQuery(Name = "name")] string? name,    [FromQuery] int top = 50,    [FromQuery(Name = "warehouseId")] int? warehouseId = null,
    [FromQuery(Name = "rackId")] int? rackId = null,    CancellationToken cancellationToken = default)

        {
            var result = await Mediator.Send(new GetBinMasterAutoComplete { SearchPattern = name , WarehouseId = warehouseId, RackId = rackId }, cancellationToken);
            if (result == null || !result.Any())   // <- check for empty
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    message = "Bin Not Found"
                });
            }

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "Bin Found",
                data = result
            });
        }

       
    }
}