using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Sales;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductionManagement.Application.RepackingHeader.Commands.CreateRepackingHeader;
using ProductionManagement.Application.RepackingHeader.Commands.DeleteRepackingHeader;
using ProductionManagement.Application.RepackingHeader.Commands.UpdateRepackingHeader;
using ProductionManagement.Application.RepackingHeader.Queries.GetAllRepackingHeader;
using ProductionManagement.Application.RepackingHeader.Queries.GetRepackingHeaderAutoComplete;
using ProductionManagement.Application.RepackingHeader.Queries.GetRepackingHeaderById;

namespace ProductionManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class RepackingHeaderController : ApiControllerBase
    {
        private readonly ISalesStockLedgerService _stockLedgerService;
        private readonly IIPAddressService _ipAddressService;

        public RepackingHeaderController(
            ISender mediator,
            ISalesStockLedgerService stockLedgerService,
            IIPAddressService ipAddressService)
            : base(mediator)
        {
            _stockLedgerService = stockLedgerService;
            _ipAddressService = ipAddressService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRepackingHeaderAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null,
            [FromQuery] int? TypeId = null)
        {
            var result = await Mediator.Send(new GetAllRepackingHeaderQuery
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                SearchTerm = SearchTerm,
                TypeId = TypeId
            });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result.Data,
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRepackingHeaderByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetRepackingHeaderByIdQuery { Id = id });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetRepackingHeaderAutoCompleteAsync(
            [FromQuery] string? term = null,
            [FromQuery] int? typeId = null)
        {
            var result = await Mediator.Send(new GetRepackingHeaderAutoCompleteQuery(term, typeId));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("getstockitems")]
        public async Task<IActionResult> GetStockItemsAsync(
            [FromQuery] int productionYear)
        {
            var unitId = _ipAddressService.GetUnitId() ?? 0;
            var result = await _stockLedgerService.GetStockItemsAsync(productionYear, unitId);
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("get-packs-by-item-lot")]
        public async Task<IActionResult> GetPacksByItemAndLotAsync(
            [FromQuery] int itemId,
            [FromQuery] int? lotId,
            [FromQuery] int productionYear)
        {
            var unitId = _ipAddressService.GetUnitId() ?? 0;
            var result = await _stockLedgerService.GetPacksByItemAndLotAsync(itemId, lotId, productionYear, unitId);
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateRepackingHeader(
            [FromBody] CreateRepackingHeaderCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result.IsSuccess,
                message = result.Message,
                data = result.Data
            });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateRepackingHeader(
            [FromBody] UpdateRepackingHeaderCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result.IsSuccess,
                message = result.Message,
                data = result.Data
            });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteRepackingHeader(int id)
        {
            var result = await Mediator.Send(new DeleteRepackingHeaderCommand(id));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }
    }
}
