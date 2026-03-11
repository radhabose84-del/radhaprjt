using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.StockLedger.Queries.GetStockLedgerReport;

namespace SalesManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class StockLedgerController : ApiControllerBase
    {
        public StockLedgerController(IMediator mediator) : base(mediator) { }

        [HttpGet("report")]
        public async Task<IActionResult> GetStockLedgerReportAsync(
            [FromQuery] int PageNumber = 1,
            [FromQuery] int PageSize = 10,
            [FromQuery] int? ItemId = null,
            [FromQuery] int? LotId = null,
            [FromQuery] int? WarehouseId = null,
            [FromQuery] int? BinId = null,
            [FromQuery] int? StatusId = null,
            [FromQuery] int? PackNo = null,
            [FromQuery] DateOnly? DateFrom = null,
            [FromQuery] DateOnly? DateTo = null)
        {
            var result = await Mediator.Send(new GetStockLedgerReportQuery
            {
                PageNumber  = PageNumber,
                PageSize    = PageSize,
                ItemId      = ItemId,
                LotId       = LotId,
                WarehouseId = WarehouseId,
                BinId       = BinId,
                StatusId    = StatusId,
                PackNo      = PackNo,
                DateFrom    = DateFrom,
                DateTo      = DateTo
            });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data       = result.Data,
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize   = result.PageSize
            });
        }
    }
}
