using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Command.CreateServiceEntrySheet;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Command.UpdateServiceEntrySheet;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.GetAllSES;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.GetApprovedPOList;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.GetPoServiceHeaderByPoId;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.GetScheduleByPoIdandSeviceidandServiceItemid;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.GetServicePOLines;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.GetServiceScheduleByPoAndServiceId;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.GetSESListToApprove;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.ServiceEntrySheetGetById;
using PurchaseManagement.Application.ServiceMaster.Queries.GetAllSES;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace PurchaseManagement.Presentation.Controllers.PurchaseOrder
{
    [ApiController]
    [Route("api/[controller]")]
    public class SESController : ApiControllerBase
    {




        public SESController(ISender mediator) : base(mediator)
        {


        }

        [HttpGet("{poId:int}/service-header", Name = "GetServicePoHeaderByPoId")]
        public async Task<IActionResult> GetServicePoHeaderByPoId([FromRoute] int poId, CancellationToken ct)
        {
            
            var dto = await Mediator.Send(new GetPoServiceHeaderByPoIdQuery { PoId = poId }, ct);
            if (dto is null)
                return NotFound(new { statusCode = 404, message = $"No service header found for PO Id={poId}." });

            return Ok(new { statusCode = 200, message = "Service header fetched successfully.", data = dto });
        }


        [HttpGet("{poId:int}/services/{serviceId:int}/schedules", Name = "GetSchedulesByPoAndServiceId")]
        public async Task<IActionResult> GetSchedulesByPoAndServiceId(
            [FromRoute] int poId,
            [FromRoute] int serviceId,
            CancellationToken ct)
        {
            var list = await Mediator.Send(
                new GetByPoAndServiceIdQuery { PoId = poId, ServiceId = serviceId }, ct);


            if (list is null || !list.Any())
                return NotFound(new
                {
                    statusCode = 404,
                    message = $"No schedules found for PO Id={poId}, ServiceId={serviceId}."
                });

                // ✅ If ALL schedules already have SES, show "already created" message
                var allSesAlreadyGenerated = list.All(s =>
                    string.Equals(s.SESAlreadyGenerated, "Yes", StringComparison.OrdinalIgnoreCase));

                var message = allSesAlreadyGenerated
                    ? "Service Entry Sheet already generated for all schedules of this PO and service."
                    : "Service schedules fetched successfully.";

            return Ok(new
            {
                statusCode = 200,
                message ,
                data = list
            });
        }

        [HttpGet("approved-list")]
        public async Task<IActionResult> GetApprovedList(CancellationToken ct)
        {
            var data = await Mediator.Send(new GetApprovedPOListQuery());
            return Ok(new { statusCode = 200, message = "Approved Service POs fetched.", data });
        }

        [HttpGet("{poId:int}/lines")]
        public async Task<IActionResult> GetServicePOLinesAsync(int poId, CancellationToken cancellationToken = default)
        {
            // call MediatR → handler → repository → SQL
            var lines = await Mediator.Send(
            new GetServicePOLinesQuery
            {
                POId = poId   // 👈 set the property, matches your query class
            },
            cancellationToken);

            // always return 200 with list (empty list is fine)
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = lines,
                message = lines.Count == 0
                    ? $"No Service PO lines found for PO ID {poId}"
                    : $"Service PO lines for PO ID {poId} fetched successfully"
            });
        }


        [HttpPost("ses")]
        public async Task<IActionResult> CreateSes([FromBody] CreateServiceSheetDto body, CancellationToken ct)
        {
            var id = await Mediator.Send(new CreateServiceEntrySheetCommand { CreateServiceSheet = body }, ct);
            return Ok(new { statusCode = 200, data = new { id }, message = "SES created" });
        }
        [HttpPut("service-entry-sheets")]
        public async Task<IActionResult> UpdateServiceEntrySheetAsync([FromBody] CreateServiceSheetDto dto, CancellationToken cancellationToken = default)
        {
            if (dto == null)
            {
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    data = (object?)null,
                    message = "Payload is required."
                });
            }

            if (dto.Id <= 0)
            {
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    data = (object?)null,
                    message = "Valid SES Id must be provided in the body."
                });
            }

            var cmd = new UpdateServiceEntrySheetCommand
            {
                Id = dto.Id,   // 🔹 SES Id comes from body
                Data = dto
            };

            var sesId = await Mediator.Send(cmd, cancellationToken);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = new { Id = sesId },
                message = $"Service Entry Sheet {sesId} updated successfully."
            });
        }
        [HttpGet("create-source")]
        public async Task<IActionResult> GetSesCreateSourceAsync(
    [FromQuery] int purchaseOrderId,
    [FromQuery] int scheduleNo,
    [FromQuery] int serviceItemId,
    CancellationToken cancellationToken)
        {
            var query = new GetSESCreateSourceQuery
            {
                PurchaseOrderId = purchaseOrderId,
                ScheduleNo = scheduleNo,
                ServiceItemId = serviceItemId
            };

            var dto = await Mediator.Send(query, cancellationToken);

            if (dto is null)
                return NotFound(
                    $"No schedule source found for PO={purchaseOrderId}, ScheduleNo={scheduleNo}, ServiceItemId={serviceItemId}.");


            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = new { dto },
                message = $"Schedule source {dto.Id} fetched successfully."
            });

        }
        [HttpGet("ses/{id:int}")]
        public async Task<IActionResult> GetSesById(int id, CancellationToken ct)
        {
            var dto = await Mediator.Send(new GetServiceEntrySheetByIdQuery { SesId = id }, ct);
            if (dto is null) return NotFound();
            return Ok(new { isSuccess = true, StatusCode = StatusCodes.Status200OK, data = dto });
        }

        [HttpGet("ses/approval")]
        public async Task<IActionResult> GetSesForApproval(
             [FromQuery] DateTimeOffset? fromDate,
             [FromQuery] DateTimeOffset? toDate,
             [FromQuery] int? vendorId,
             CancellationToken ct)
        {
            var list = await Mediator.Send(new GetSesApprovalListQuery
            {
                FromDate = fromDate,
                ToDate = toDate,
                VendorId = vendorId
            }, ct);

            return Ok(new
            {
                isSuccess = true,
                statusCode = StatusCodes.Status200OK,
                data = list
            });
        }

        [HttpGet("ses/with-activities")]
        public async Task<IActionResult> GetSesWithActivities(
            [FromQuery] int purchaseOrderId,
            CancellationToken ct)
        {
            // If your query has a constructor: new GetServiceEntrySheetsWithActivitiesByPoIdQuery(purchaseOrderId)
            var result = await Mediator.Send(
                new GetServiceEntrySheetsWithActivitiesByPoIdQuery(purchaseOrderId),
                ct);

            return Ok(new
            {
                isSuccess = result.IsSuccess,
                statusCode = StatusCodes.Status200OK,   // you can change this based on result if you want
                message = result.Message,
                data = result.Data
            });
        }


        [HttpGet("ses/list")]
        public async Task<IActionResult> GetAllSesList(
           [FromQuery] int pageNumber = 1,
           [FromQuery] int pageSize = 10,
           [FromQuery] string? searchTerm = null,
           CancellationToken ct = default)
        {
            var result = await Mediator.Send(new GetSESListQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm
            }, ct);

            return Ok(new
            {
                isSuccess = result.IsSuccess,
                statusCode = StatusCodes.Status200OK,
                message = result.Message,
                totalCount = result.TotalCount,
                pageNumber = result.PageNumber,
                pageSize = result.PageSize,
                data = result.Data
            });
        }
        

        [HttpGet("ses/details")]
            public async Task<IActionResult> GetSesFullDetails( [FromQuery] int sesId, CancellationToken ct)
            {
                var result = await Mediator.Send(new GetServiceEntrySheetByIdQuery
                {
                    SesId  = sesId
                }, ct);

                return Ok(new
                {
                    isSuccess  = result.IsSuccess,
                    statusCode = result.StatusCode,
                    message    = result.Message,
                    data       = result.Data
                });
            }

    }
}