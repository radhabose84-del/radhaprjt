using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.CostCenter.Command.CreateCostCenter;
using MaintenanceManagement.Application.CostCenter.Command.DeleteCostCenter;
using MaintenanceManagement.Application.CostCenter.Command.UpdateCostCenter;
using MaintenanceManagement.Application.CostCenter.Queries.GetCostCenter;
using MaintenanceManagement.Application.CostCenter.Queries.GetCostCenterAutoComplete;
using MaintenanceManagement.Application.CostCenter.Queries.GetCostCenterById;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MaintenanceManagement.API.Controllers
{
    [Route("api/[controller]")]
    public class CostCenterController : ApiControllerBase
    {

        private readonly IMediator _mediator;


        public CostCenterController(IMediator mediator)
        : base(mediator)
        {

            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCostcenterAsync([FromQuery] int PageNumber, [FromQuery] int PageSize, [FromQuery] string? SearchTerm = null)
        {
            var costcenter = await Mediator.Send(
             new GetCostCenterQuery
             {
                 PageNumber = PageNumber,
                 PageSize = PageSize,
                 SearchTerm = SearchTerm
             });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = costcenter.Data,
                TotalCount = costcenter.TotalCount,
                PageNumber = costcenter.PageNumber,
                PageSize = costcenter.PageSize
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetCostcenter([FromQuery] string? CostCenterName)
        {
            var costcenter = await Mediator.Send(new GetCostCenterAutoCompleteQuery
            {
                SearchPattern = CostCenterName ?? string.Empty
            });

            return Ok(new { StatusCode = StatusCodes.Status200OK, data = costcenter });
        }

        [HttpGet("{id}")]
        [ActionName(nameof(GetByIdAsync))]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var costcenter = await Mediator.Send(new GetCostCenterByIdQuery() { Id = id });

            return Ok(new { StatusCode = StatusCodes.Status200OK, data = costcenter, message = costcenter });

        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateCostCenterCommand createCostCenterCommand)
        {

            var CreatedCostCenterId = await _mediator.Send(createCostCenterCommand);


            return Ok(new
            {
                StatusCode = StatusCodes.Status201Created,
                message = "Created successfully.",
                data = CreatedCostCenterId
            });


        }
        [HttpPut]
        public async Task<IActionResult> UpdateAsync(UpdateCostCenterCommand updateCostCenterCommand)
        {

            await _mediator.Send(updateCostCenterCommand);

            return Ok(new
            {
                message = "Updated successfully.",
                statusCode = StatusCodes.Status200OK
            });

        }

        [HttpDelete]
        public async Task<IActionResult> DeleteCostCenterAsync(int id)
        {

            await _mediator.Send(new DeleteCostCenterCommand { Id = id });
            return Ok(new
            {
                message = "Deleted successfully.",
                statusCode = StatusCodes.Status200OK
            });

        }

    }
}