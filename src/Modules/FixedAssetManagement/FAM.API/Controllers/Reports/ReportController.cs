using FAM.Application.Reports.AssetAudit;
using FAM.Application.Reports.AssetReport;
using FAM.Application.Reports.AssetTransferReport;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FAM.API.Controllers.Reports
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ApiControllerBase
    {

        public ReportController(ISender mediator)
        : base(mediator)
        {

        }

        [HttpGet("AssetReport")]
        public async Task<IActionResult> AssetReportAsync([FromQuery] DateTimeOffset? fromDate = null,
            [FromQuery] DateTimeOffset? toDate = null)
        {
            /*   DateTimeOffset? parsedFromDate = null;
               DateTimeOffset? parsedToDate = null;

                if (!string.IsNullOrWhiteSpace(fromDate))  // Allow null or empty values
               {
                   if (!DateTimeOffset.TryParse(fromDate, out var parsedDate))
                   {
                       return BadRequest(new { message = "Invalid fromDate format. Use yyyy-MM-dd." });
                   }
                   parsedFromDate = parsedDate;
               }

               if (!string.IsNullOrWhiteSpace(toDate))  // Allow null or empty values
               {
                   if (!DateTimeOffset.TryParse(toDate, out var parsedDate))
                   {
                       return BadRequest(new { message = "Invalid toDate format. Use yyyy-MM-dd." });
                   }
                   parsedToDate = parsedDate;
               } */

            var query = new AssetReportQuery
            {
                FromDate = fromDate,
                ToDate = toDate
            };
            var result = await Mediator.Send(query);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                Message = result?.Message ?? "No Asset Report found.",
                Data = result?.Data ?? new List<AssetReportDto>()
            });
        }

        [HttpGet("AssetTransferReport")]
        public async Task<IActionResult> AssetTransferReportAsync(
            [FromQuery] DateTimeOffset? FromDate = null,
            [FromQuery] DateTimeOffset? ToDate = null)
        {
            var result = await Mediator.Send(new AssetTransferQuery
            {
                FromDate = FromDate,
                ToDate = ToDate
            });

            if (result?.Data == null || result.Data.Count == 0)
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = result?.Message ?? "No Asset Transfer Report found.",
                    Data = new List<AssetTransferDetailsDto>()
                });
            }

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                Message = result.Message,
                Data = result.Data?.ToList() ?? new List<AssetTransferDetailsDto>()

            });
        }
         [HttpGet("AssetAuditReport")]
        public async Task<IActionResult> AssetAuditReportAsync(
            [FromQuery] int auditTypeId)
        {
            var result = await Mediator.Send(new AssetAuditReportQuery
            {
                AuditCycle = auditTypeId
           
            });

            if (result?.Data == null || result.Data.Count == 0)
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = result?.Message ?? "No Asset Audit Report found.",
                    Data = new List<AssetAuditReportDto>()
                });
            }

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                Message = result.Message,
                Data = result.Data
            });
        }

    }
}