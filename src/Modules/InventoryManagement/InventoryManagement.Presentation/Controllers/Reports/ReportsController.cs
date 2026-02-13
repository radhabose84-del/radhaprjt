using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using InventoryManagement.Application.Reports.GetUnitsByDivision;
using InventoryManagement.Application.Reports.StockReport;
using InventoryManagement.Application.Reports.StockReportDivisionwise;
using InventoryManagement.Application.Reports.SubStoresStock;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace InventoryManagement.Presentation.Controllers.Reports
{
    [ApiController]
    [Route("api/[controller]")]
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

        [HttpGet("CurrentStockUnitWise")]
        [ActionName(nameof(GetCurrentStockUnitWiseSummary))]
        public async Task<IActionResult> GetCurrentStockUnitWiseSummary(
             [FromQuery] string unitIds,
             int? itemId = null,
             int? warehouseId = null,
             int? storageTypeId = null,
             int? targetId = null)
        {
            if (string.IsNullOrWhiteSpace(unitIds))
                return BadRequest("UnitIds are required.");

            var parsedIds = unitIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(id => int.TryParse(id.Trim(), out var v) ? v : (int?)null)
                .Where(v => v.HasValue)
                .Select(v => v!.Value)
                .ToList();

            if (!parsedIds.Any())
                return BadRequest("Invalid UnitIds.");

            var result = await Mediator.Send(
                new GetStockReportDivsionwiseSummaryQuery
                {
                    UnitIds = parsedIds,
                    ItemId = itemId,
                    WarehouseId = warehouseId,
                    StorageTypeId = storageTypeId,
                    TargetId = targetId
                });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result,
                message = "Current stock fetched successfully"
            });
        }
          [HttpGet("by-division")]
        public async Task<IActionResult> GetUnitsByDivision(
            [FromQuery] int companyId,
            [FromQuery] int divisionId)
        {
            if (companyId <= 0 || divisionId <= 0)
                return BadRequest("CompanyId and DivisionId are required.");

            var result = await Mediator.Send(
                new GetUnitsByDivisionQuery
                {
                    CompanyId = companyId,
                    DivisionId = divisionId
                });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result,
                message = "Units fetched successfully"
            });
        }

    }
}