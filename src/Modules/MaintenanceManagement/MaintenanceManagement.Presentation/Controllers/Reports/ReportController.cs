using MaintenanceManagement.Application.Reports.GetCurrentAllStockItems;
using MaintenanceManagement.Application.Reports.GetStockLegerReport;
using MaintenanceManagement.Application.Reports.MaintenanceRequestReport;
using MaintenanceManagement.Application.Reports.WorkOrderItemConsuption;
using MaintenanceManagement.Application.Reports.WorkOrderReport;
using MaintenanceManagement.Application.Reports.WorkOderCheckListReport;
using MaintenanceManagement.Presentation.Controllers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MaintenanceManagement.Application.Reports.MRS;
using MaintenanceManagement.Application.Reports.ScheduleReport;
using MaintenanceManagement.Application.Reports.MaterialPlanningReport;
using MaintenanceManagement.Application.Reports.PowerConsumption;
using MaintenanceManagement.Application.Reports.GeneratorConsumption;
using Microsoft.AspNetCore.Http;

namespace MaintenanceManagement.Presentation.Controllers.Reports
{
    [Route("api/maintenance/[controller]")]

    public class ReportController : ApiControllerBase
    {


        public ReportController(ISender mediator)
        : base(mediator)
        {

        }




        [HttpGet("WorkOrderReport")]

        public async Task<IActionResult> WorkOrderReportAsync([FromQuery] string? fromDate, [FromQuery] string? toDate, [FromQuery] int requestTypeId ,[FromQuery] int? departmentId)
        {
            DateTimeOffset? parsedFromDate = null;
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
            }

            var query = new WorkOrderReportQuery
            {
                FromDate = parsedFromDate,

                ToDate = parsedToDate,
                RequestTypeId = requestTypeId,
                DepartmentId=departmentId??0
            };
            var result = await Mediator.Send(query);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                Message = result?.Message ?? "No Asset Report found.",
                Data = result?.Data ?? new List<WorkOrderReportDto>()
            });
        }

        [HttpGet("ItemConsumption")]
        public async Task<IActionResult> GetAllItemConsuption(
            [FromQuery] string? fromDate,
            [FromQuery] string? toDate)
        {
            DateTimeOffset? parsedFromDate = null;
            DateTimeOffset? parsedToDate = null;

            if (!string.IsNullOrWhiteSpace(fromDate))
            {
                if (!DateTimeOffset.TryParse(fromDate, out var fromParsed))
                {
                    return BadRequest(new { message = "Invalid fromDate format. Use yyyy-MM-dd." });
                }
                parsedFromDate = fromParsed;
            }

            if (!string.IsNullOrWhiteSpace(toDate))
            {
                if (!DateTimeOffset.TryParse(toDate, out var toParsed))
                {
                    return BadRequest(new { message = "Invalid toDate format. Use yyyy-MM-dd." });
                }
                parsedToDate = toParsed;
            }



            var workOrder = await Mediator.Send(new WorkOrderIssueQuery
            {
                IssueFrom = parsedFromDate,
                IssueTo = parsedToDate

            });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = workOrder.Message,
                data = workOrder.Data?.ToList()
            });
        }

        [HttpGet("SubStoresStockLedger")]
        [ActionName(nameof(GetSubStoresStockLedger))]
        public async Task<IActionResult> GetSubStoresStockLedger(
            [FromQuery] string oldUnitcode,
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate,
            [FromQuery] int DepartmentId,
            [FromQuery] string? itemcode = null
            )
        {
            // Manual validation
            if (string.IsNullOrWhiteSpace(oldUnitcode))
            {
                return BadRequest(new
                {
                    statusCode = StatusCodes.Status400BadRequest,
                    message = "'oldUnitcode' query parameter is required."
                });
            }

            if (fromDate > toDate)
            {
                return BadRequest(new
                {
                    statusCode = StatusCodes.Status400BadRequest,
                    message = "'fromDate' must be less than or equal to 'toDate'."
                });
            }

            if (!IsSameFinancialYear(fromDate, toDate))
            {
                return BadRequest(new
                {
                    statusCode = StatusCodes.Status400BadRequest,
                    message = "'fromDate' and 'toDate' must fall within the same financial year (April to March)."
                });
            }

            var result = await Mediator.Send(new GetStockLegerReportQuery { OldUnitcode = oldUnitcode, FromDate = fromDate, ToDate = toDate, ItemCode = itemcode, DepartmentId = DepartmentId });

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(new
                {
                    statusCode = StatusCodes.Status200OK,
                    data = result.Data?.ToList(),
                    message = "Success"
                });
            }

            return NotFound(new
            {
                statusCode = StatusCodes.Status404NotFound,
                message = "No stock ledger data found for the given criteria."
            });
        }

        // Ensure both dates fall in the same financial year (April 1 to March 31)
        bool IsSameFinancialYear(DateTime date1, DateTime date2)
        {
            int fyStartYear1 = date1.Month >= 4 ? date1.Year : date1.Year - 1;
            int fyStartYear2 = date2.Month >= 4 ? date2.Year : date2.Year - 1;
            return fyStartYear1 == fyStartYear2;
        }
        [HttpGet("CurrentStock/{oldUnitCode}/{departmentId}")]
        [ActionName(nameof(GetAllStockItemDetails))]
        public async Task<IActionResult> GetAllStockItemDetails(string oldUnitCode, int departmentId)
        {
            var result = await Mediator.Send(new GetCurrentAllStockItemsQuery { OldUnitcode = oldUnitCode, DepartmentId = departmentId });

            if (result.IsSuccess)
            {
                return Ok(new
                {
                    StatusCode = StatusCodes.Status200OK,
                    data = result.Data,
                    message = result.Message
                });
            }

            return NotFound(new
            {
                StatusCode = StatusCodes.Status404NotFound,
                message = result.Message
            });
        }

        // [HttpGet("RequestReport")]
        // public async Task<IActionResult> MaintenanceReportAsync(
        //             [FromQuery] DateTimeOffset? requestFromDate,
        //             [FromQuery] DateTimeOffset? requestToDate,
        //             [FromQuery] int? RequestType,
        //             [FromQuery] int? requestStatus,
        //             [FromQuery] int? departmentId
        //             )
        // {
        //     var query = new RequestReportQuery
        //     {
        //         RequestFromDate = requestFromDate,
        //         RequestToDate = requestToDate,
        //         RequestType = RequestType,
        //         RequestStatus = requestStatus,
        //         DepartmentId = departmentId
        //     };

        //     var result = await Mediator.Send(query);

        //     if (result == null || result.Data == null || result.Data.Count == 0)
        //     {
        //         return NotFound(new
        //         {
        //             StatusCode = StatusCodes.Status404NotFound,
        //             Message = result?.Message ?? "No maintenance requests found."

        //         });
        //     }

        //     return Ok(new
        //     {
        //         StatusCode = StatusCodes.Status200OK,
        //         Message = result.Message,
        //         Data = result?.Data?.ToList()
        //     });
        // }
        [HttpGet("RequestReport")]
        public async Task<IActionResult> MaintenanceReportAsync(
        [FromQuery] DateTimeOffset? requestFromDate,
        [FromQuery] DateTimeOffset? requestToDate,
        [FromQuery] int? RequestType,
        [FromQuery] int? requestStatus,
        [FromQuery] int? departmentId)
        {
            var query = new RequestReportQuery
            {
                RequestFromDate = requestFromDate,
                RequestToDate = requestToDate,
                RequestType = RequestType,
                RequestStatus = requestStatus,
                DepartmentId = departmentId
            };

            var result = await Mediator.Send(query);

            // Always return 200 OK, even if IsSuccess is false
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                Message = result.Message,
                Data = result.Data ?? new List<RequestReportDto>()

            });
        }
        [HttpGet("WorkOrderChecklistReport")]
        public async Task<IActionResult> WorkOrderChecklistReportAsync(
            [FromQuery] DateTimeOffset? WorkOrderFromDate,
            [FromQuery] DateTimeOffset? WorkOrderToDate,
            [FromQuery] int? MachineGroupId,
            [FromQuery] int? machineId,
            [FromQuery] int? ActivityId)
        {
            var query = new WorkOderCheckListReportQuery
            {
                WorkOrderFromDate = WorkOrderFromDate,
                WorkOrderToDate = WorkOrderToDate,
                MachineGroupId = MachineGroupId,
                MachineId = machineId,
                ActivityId = ActivityId
            };

            var result = await Mediator.Send(query);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                Message = result?.Message ?? "No Work Order Checklist records found.",
                Data = result?.Data ?? new List<WorkOderCheckListReportDto>()

            });
        }



        // [HttpGet("WorkOrderChecklistReport")]
        // public async Task<IActionResult> WorkOrderChecklistReportAsync(
        //         [FromQuery] DateTimeOffset? WorkOrderFromDate,
        //         [FromQuery] DateTimeOffset? WorkOrderToDate,
        //         [FromQuery] int? MachineGroupId,
        //         [FromQuery] int? machineId,
        //         [FromQuery] int? ActivityId
        //         )
        // {
        //     var query = new WorkOderCheckListReportQuery
        //     {
        //         WorkOrderFromDate = WorkOrderFromDate,
        //         WorkOrderToDate = WorkOrderToDate,
        //         MachineGroupId = MachineGroupId,
        //         MachineId = machineId,
        //         ActivityId = ActivityId
        //     };

        //     var result = await Mediator.Send(query);

        //     if (result == null || result.Data == null || result.Data.Count == 0)
        //     {
        //         return NotFound(new
        //         {
        //             StatusCode = StatusCodes.Status404NotFound,
        //             Message = result?.Message ?? "No Work Order Checklist records found."
        //         });
        //     }

        //     return Ok(new
        //     {
        //         StatusCode = StatusCodes.Status200OK,
        //         Message = result.Message,
        //         //Data = result.Data
        //        // Data = result?.Data?.ToList()
        //         Data = result?.Data?.ToList()
        //     });
        // }

        [HttpGet("MRSReport")]
        public async Task<IActionResult> GetMRSReport(
           [FromQuery] string? fromDate,
           [FromQuery] string? toDate,
           [FromQuery] string? OldUnitCode)
        {
            DateTimeOffset? parsedFromDate = null;
            DateTimeOffset? parsedToDate = null;

            if (!string.IsNullOrWhiteSpace(fromDate))
            {
                if (!DateTimeOffset.TryParse(fromDate, out var fromParsed))
                {
                    return BadRequest(new { message = "Invalid fromDate format. Use yyyy-MM-dd." });
                }
                parsedFromDate = fromParsed;
            }

            if (!string.IsNullOrWhiteSpace(toDate))
            {
                if (!DateTimeOffset.TryParse(toDate, out var toParsed))
                {
                    return BadRequest(new { message = "Invalid toDate format. Use yyyy-MM-dd." });
                }
                parsedToDate = toParsed;
            }

            if (OldUnitCode is null)
            {
                return BadRequest(new { message = "Invalid OldUnitCode" });
            }

            var workOrder = await Mediator.Send(new MRSReportQuery
            {
                FromDate = parsedFromDate,
                ToDate = parsedToDate,
                OldUnitCode = OldUnitCode
            });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = workOrder.Message,
                data = workOrder.Data?.ToList()
            });
        }

        [HttpGet("SchedulerReport")]
        public async Task<IActionResult> SchedulerReportAsync(
               [FromQuery] DateTime? FromDueDate,
                [FromQuery] DateTime? ToDueDate
               )
        {
            var query = new ScheduleReportQuery
            {
                FromDueDate = FromDueDate,
                ToDueDate = ToDueDate
            };

            var result = await Mediator.Send(query);



            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                Message = result?.Message,
                Data = result?.Data ?? new List<ScheduleReportDto>()
            });
        }
        [HttpGet("MaterialPlanningReport")]
        public async Task<IActionResult> MaterialPlanningReportAsync(
             [FromQuery] DateTime? FromDueDate,
             [FromQuery] DateTime? ToDueDate
             )
        {
            var query = new MaterialPlanningReportQuery
            {
                FromDueDate = FromDueDate,
                ToDueDate = ToDueDate
            };

            var result = await Mediator.Send(query);


            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                Message = result.Message,
                Data = result?.Data ?? new List<MaterialPlanningReportDto>()
            });
        }
        [HttpGet("PowerConsumptionReport")]
        public async Task<IActionResult> AssetTransferReportAsync(
            [FromQuery] DateTimeOffset? FromDate = null,
            [FromQuery] DateTimeOffset? ToDate = null)
        {
            var result = await Mediator.Send(new PowerConsumptionReportQuery
            {
                FromDate = FromDate,
                ToDate = ToDate
            });

            if (result?.Data == null || result.Data.Count == 0)
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = result?.Message ?? "Power Transactions not found.",
                    Data = new List<PowerReportDto>()
                });
            }

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                Message = result.Message,
                Data = result.Data?.ToList() ?? new List<PowerReportDto>()

            });
        }
        [HttpGet("GeneratorConsumptionReport")]
        public async Task<IActionResult> GeneratorConsumptionReportAsync(
            [FromQuery] DateTimeOffset? FromDate = null,
            [FromQuery] DateTimeOffset? ToDate = null)
        {
            var result = await Mediator.Send(new GeneratorConsumptionReportQuery
            {
                FromDate = FromDate,
                ToDate = ToDate
            });

            if (result?.Data == null || result.Data.Count == 0)
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = result?.Message ?? "Generator Transactions not found.",
                    Data = new List<GeneratorReportDto>()
                });
            }

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                Message = result.Message,
                Data = result.Data?.ToList() ?? new List<GeneratorReportDto>()
               
            });
        }
    }
}