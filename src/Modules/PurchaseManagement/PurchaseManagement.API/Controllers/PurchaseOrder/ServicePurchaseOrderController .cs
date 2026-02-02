using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Application.PurchaseOrder.Dtos.ServicePO;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Command.Create;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Command.CreateServiceEntrySheet;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Command.Update;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.GetServicePO;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.GetServicePOPending;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.GetVendorServicePO;
using PurchaseManagement.Application.PurchaseOrder.SevicePOAmendment;
using PurchaseManagement.Application.ServiceMaster.Commands.CreateService;
using MassTransit.Futures.Contracts;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace PurchaseManagement.API.Controllers.PurchaseOrder
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServicePurchaseOrderController : ApiControllerBase
    {


        private readonly ILogger<ServicePurchaseOrderController> _logger;


        public ServicePurchaseOrderController(ISender mediator) : base(mediator)
        {


        }

       [HttpPost]
    public async Task<IActionResult> CreateServicePo([FromBody] CreateServicePurchaseOrderDto body, CancellationToken ct)
        {
            var id = await Mediator.Send(new CreateServicePoCommand { Data = body }, ct);
            if (id <= 0)
                return BadRequest(new { statusCode = 400, message = "Create failed.", data = (object?)null });

            return CreatedAtRoute(
                routeName: "GetServicePoById",     // <-- must match the Name on GET
                routeValues: new { id },           // <-- must match {id:int}
                value: new { statusCode = 201, message = "Created successfully.", data = new { id } }
            );
        }
    

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] CreateServicePurchaseOrderDto dto, CancellationToken ct)
        {
            // Check if ID is provided in the payload
            if (dto == null || dto.Id == 0)
            {
                return BadRequest(new
                {
                    message = "Invalid data provided. Please ensure the ID is provided in the request body.",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            var success = await Mediator.Send(new UpdateServicePoCommand { Data = dto }, ct);

            if (!success)
            {
                return NotFound(new
                {
                    message = $"Service PO with Id {dto.Id} not found.",
                    StatusCode = StatusCodes.Status404NotFound
                });
            }

            return Ok(new
            {
                message = "Service PO updated successfully.",
                StatusCode = StatusCodes.Status200OK,
                data = new { dto.Id }
            });
        }

        [HttpGet("{id:int}", Name = "GetServicePoById")]
        public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken ct = default)
        {
            var dto = await Mediator.Send(new GetServicePOByIdQuery(id), ct);
            if (dto is null)
                return NotFound(new { statusCode = 404, message = "Service PO not found." });

            return Ok(new { statusCode = 200, Message = "Service PO fetched successfully.", data = dto });
        }

        [HttpPost("amend")]
        public async Task<IActionResult> Amend([FromBody] CreateServicePurchaseOrderDto dto, CancellationToken ct)
        {
            if (dto is null) return BadRequest(new { statusCode = 400, message = "Body is required." });
            if (dto.Id <= 0) return BadRequest(new { statusCode = 400, message = "Id is required for amendment." });

            var cmd = new SevicePOAmendmentCommand { Dto = dto };   // ← change here
            var newId = await Mediator.Send(cmd, ct);
            return Ok(new { statusCode = 200, data = new { newId } , message = "Amendment created successfully." });
        }

      

        [HttpGet("ServicePO/{vendorId}")]
        public async Task<IActionResult> GetVendorServicePO(int vendorId)
        {
            var servicePo = await Mediator.Send(new GetVendorServicePOQuery() { VendorId = vendorId });

            if (servicePo == null)
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    data = (object?)null,
                    message = $"PurchaseOrder with ID {vendorId} not found"
                });
            }

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = servicePo,
                message = "ID fetched successfully"
            });
        }


        [HttpGet("pending-service-po")]
        public async Task<IActionResult> GetPendingServicePOAsync(
            [FromQuery] int? poId = null,
            [FromQuery] int? pageNumber = 1,
            [FromQuery] int? pageSize = 15,
            [FromQuery] string? searchTerm = null,
            CancellationToken cancellationToken = default)
        {
            // Optional: normalize paging
            var page = (pageNumber is > 0) ? pageNumber : 1;
            var size = (pageSize is > 0) ? pageSize : 15;

            var (items, total) = await Mediator.Send(new GetPOServicePendingQuery
            {
                PoId = poId,
                PageNumber = page,
                PageSize = size,
                SearchTerm = searchTerm
            }, cancellationToken);       

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = new { Items = items, TotalCount = total },
                message = $"Pending Service PO data{(poId.HasValue ? $" for PO ID {poId}" : "")} fetched successfully"
            });
        }
            


        






    }
}