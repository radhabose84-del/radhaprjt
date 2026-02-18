using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MaintenanceManagement.Application.MRS.Command.CreateMRS;
using MaintenanceManagement.Application.MRS.Queries;
using MaintenanceManagement.Application.MRS.Queries.GetCategory;
using MaintenanceManagement.Application.MRS.Queries.GetPendingQty;
using MaintenanceManagement.Application.MRS.Queries.GetSubCostCenter;
using MaintenanceManagement.Application.MRS.Queries.GetSubDepartment;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MaintenanceManagement.Presentation.Controllers
{
    [Route("api/maintenance/[controller]")]
    public class MRSController : ApiControllerBase
    {
        
        private readonly IMediator _mediator;

        public MRSController( IMediator mediator)
         : base(mediator)
        {
            
            _mediator = mediator;
            
        }

        [HttpGet("department/{oldUnitcode}")]
        [ActionName(nameof(GetDepartment))]
        public async Task<IActionResult> GetDepartment(string oldUnitcode)
        {
            var stockItem = await _mediator.Send(new GetDepartmentbyIdQuery
            {
                OldUnitcode = oldUnitcode

            });

           
                return Ok(new
                {
                    StatusCode = StatusCodes.Status200OK,
                    data = stockItem,
                    message = stockItem
                });
            
        }
        [HttpGet("SubCostCenter/{oldUnitcode}")]
        [ActionName(nameof(GetSubCostCenter))]
        public async Task<IActionResult> GetSubCostCenter(string oldUnitcode)
        {
            var stockItem = await _mediator.Send(new GetSubCostCenterQuery
            {
                OldUnitcode = oldUnitcode

            });

           
                return Ok(new
                {
                    StatusCode = StatusCodes.Status200OK,
                    data = stockItem,
                    message = stockItem
                });
            
        }

        [HttpGet("Category/{oldUnitcode}")]
        [ActionName(nameof(GetCategory))]
        public async Task<IActionResult> GetCategory(string oldUnitcode)
        {
            var stockItem = await _mediator.Send(new GetCategoryQuery
            {
                OldUnitcode = oldUnitcode

            });

            
                return Ok(new
                {
                    StatusCode = StatusCodes.Status200OK,
                    data = stockItem,
                    message = stockItem
                });
           
        }
        [HttpGet("SubDepartment/{oldUnitcode}")]
        [ActionName(nameof(GetSubDepartment))]
        public async Task<IActionResult> GetSubDepartment(string oldUnitcode)
        {
            var stockItem = await _mediator.Send(new GetSubDepartmentQuery
            {
                OldUnitcode = oldUnitcode

            });

            
                return Ok(new
                {
                    StatusCode = StatusCodes.Status200OK,
                    data = stockItem,
                    message = stockItem
                });
            
        }
        [HttpPost("CreateMRS")]
        public async Task<IActionResult> CreateMRS([FromBody] HeaderRequest headerRequest)
        {
           

            var command = new CreateMRSCommand { Header = headerRequest };

            var result = await _mediator.Send(command);

                return Ok(new
                {
                    StatusCode = StatusCodes.Status201Created,
                    message = "MRS Created Successfully",
                    data = result
                });
           
        }

        [HttpGet("pending-issue")]
        public async Task<IActionResult> GetPendingIssue([FromQuery] string oldUnitCode, [FromQuery] string itemCode)
        {
            var result = await _mediator.Send(new GetPendingQtyQuery
            {
                OldUnitcode = oldUnitCode,
                ItemCode = itemCode
            });


                return Ok(new
                {
                    StatusCode = StatusCodes.Status200OK,
                    data = result ?? new GetPendingQtyDto { PendingQty = 0 },
                    message = result
                });
            

            
        }

       
    }
}