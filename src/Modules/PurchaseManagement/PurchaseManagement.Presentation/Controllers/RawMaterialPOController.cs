using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.RawMaterialPO.Commands.CreateRawMaterialPO;
using PurchaseManagement.Application.RawMaterialPO.Commands.DeleteRawMaterialPO;
using PurchaseManagement.Application.RawMaterialPO.Commands.UpdateRawMaterialPO;
using PurchaseManagement.Application.RawMaterialPO.Queries.GetAllRawMaterialPO;
using PurchaseManagement.Application.RawMaterialPO.Queries.GetRawMaterialPOAutoComplete;
using PurchaseManagement.Application.RawMaterialPO.Queries.GetRawMaterialPOById;
using PurchaseManagement.Application.RawMaterialPO.Queries.GetRawMaterialPOFromOcr;

namespace PurchaseManagement.Presentation.Controllers
{
    public class RawMaterialPOController : ApiControllerBase
    {
        public RawMaterialPOController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllRawMaterialPOAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllRawMaterialPOQuery
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                SearchTerm = SearchTerm
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
        public async Task<IActionResult> GetRawMaterialPOByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetRawMaterialPOByIdQuery { Id = id });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetRawMaterialPOAutoCompleteAsync([FromQuery] string term = "")
        {
            var result = await Mediator.Send(new GetRawMaterialPOAutoCompleteQuery(term));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        // Auto-fetch approved-OCR details + RemainingQuantity ("Max N Bales") for the conversion screen
        [HttpGet("from-ocr/{ocrId}")]
        public async Task<IActionResult> GetRawMaterialPOFromOcrAsync(int ocrId)
        {
            var result = await Mediator.Send(new GetRawMaterialPOFromOcrQuery { OcrId = ocrId });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateRawMaterialPO([FromBody] CreateRawMaterialPOCommand command)
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
        public async Task<IActionResult> UpdateRawMaterialPO([FromBody] UpdateRawMaterialPOCommand command)
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
        public async Task<IActionResult> DeleteRawMaterialPO(int id)
        {
            var result = await Mediator.Send(new DeleteRawMaterialPOCommand(id));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Raw Material PO deleted successfully." : "Raw Material PO not found."
            });
        }
    }
}
