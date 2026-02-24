using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Application.Reports.StockReport;
using PurchaseManagement.Application.Reports.SubStoresStock;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace PurchaseManagement.Presentation.Controllers.Reports
{
    [Route("api/purchase/[controller]")]
    public class ReportsController : ApiControllerBase
    {

        public ReportsController(ISender mediator)
          : base(mediator)
        {

        }
        [HttpGet("CurrentStock")]
        [ActionName(nameof(GetCurrentStockSummary))]
        public async Task<IActionResult> GetCurrentStockSummary(
            int? itemId = null,
            int? warehouseId = null,
            int? storageTypeId = null,
            int? targetId = null)
        {
            var result = await Mediator.Send(new GetStockReportSummaryQuery
            {
                ItemId = itemId,
                WarehouseId = warehouseId,
                StorageTypeId = storageTypeId,
                TargetId = targetId
            });

            if (result == null)
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    data = (object?)null,
                    message = $"StockReport with ID not found"
                });
            }

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result,
                message = "ID fetched successfully"
            });
        }
        
        [HttpGet("SubStoresCurrentStock")]
        [ActionName(nameof(GetCurrentSubStoresStockSummary))]
        public async Task<IActionResult> GetCurrentSubStoresStockSummary(
            int? itemId = null,
            int? departmentId = null,
            int? warehouseId = null,
            int? storageTypeId = null,
            int? targetId = null)
        {
            var result = await Mediator.Send(new GetSubStockReportSummaryQuery
            {
                ItemId = itemId,
                DepartmentId = departmentId,
                WarehouseId = warehouseId,
                StorageTypeId = storageTypeId,
                TargetId = targetId
            });

              if (result == null)
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    data = (object?)null,
                    message = $"Sub Stores StockReport with ID not found"
                });
            }

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result,
                message = "ID fetched successfully"
            });
        }

        
    }
}