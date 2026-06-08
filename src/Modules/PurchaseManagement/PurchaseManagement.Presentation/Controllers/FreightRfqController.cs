using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.FreightRfq.Commands.ApproveFreightRfq;
using PurchaseManagement.Application.FreightRfq.Commands.CreateFreightRfq;
using PurchaseManagement.Application.FreightRfq.Commands.DeleteFreightRfq;
using PurchaseManagement.Application.FreightRfq.Commands.RejectFreightRfq;
using PurchaseManagement.Application.FreightRfq.Commands.SaveFreightRfqQuotations;
using PurchaseManagement.Application.FreightRfq.Commands.SubmitFreightRfqForApproval;
using PurchaseManagement.Application.FreightRfq.Commands.UpdateFreightRfq;
using PurchaseManagement.Application.FreightRfq.Queries.GetAllFreightRfq;
using PurchaseManagement.Application.FreightRfq.Queries.GetFreightRfqById;
using PurchaseManagement.Application.FreightRfq.Queries.GetFreightRfqPoPrefill;
using PurchaseManagement.Application.FreightRfq.Queries.GetFreightRfqTransporters;
using PurchaseManagement.Application.FreightRfq.Queries.GetNextFreightRfqNumber;
using PurchaseManagement.Application.FreightRfq.Queries.GetPendingPoReferences;

namespace PurchaseManagement.Presentation.Controllers
{
    [Route("api/purchase/[controller]")]
    public class FreightRfqController : ApiControllerBase
    {
        public FreightRfqController(ISender mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null,
            [FromQuery] int? StatusId = null)
        {
            var result = await Mediator.Send(new GetAllFreightRfqQuery
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                SearchTerm = SearchTerm,
                StatusId = StatusId
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
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetFreightRfqByIdQuery { Id = id });
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result });
        }

        [HttpGet("po-references")]
        public async Task<IActionResult> GetPendingPoReferencesAsync([FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetPendingPoReferencesQuery(term));
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result });
        }

        [HttpGet("po-prefill/{poId}")]
        public async Task<IActionResult> GetPoPrefillAsync(int poId)
        {
            var result = await Mediator.Send(new GetFreightRfqPoPrefillQuery(poId));
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result });
        }

        [HttpGet("transporters")]
        public async Task<IActionResult> GetTransportersAsync([FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetFreightRfqTransportersQuery(term));
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result });
        }

        [HttpGet("next-number")]
        public async Task<IActionResult> GetNextNumberAsync([FromQuery] DateTimeOffset? rfqDate = null)
        {
            var result = await Mediator.Send(new GetNextFreightRfqNumberQuery
            {
                RfqDate = rfqDate ?? DateTimeOffset.Now
            });
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = new { nextNumber = result } });
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] CreateFreightRfqCommand command)
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
        public async Task<IActionResult> UpdateAsync([FromBody] UpdateFreightRfqCommand command)
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

        [HttpPut("quotations")]
        public async Task<IActionResult> SaveQuotationsAsync([FromBody] SaveFreightRfqQuotationsCommand command)
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

        [HttpPost("submit-for-approval")]
        public async Task<IActionResult> SubmitForApprovalAsync([FromBody] SubmitFreightRfqForApprovalCommand command)
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

        [HttpPost("approve")]
        public async Task<IActionResult> ApproveAsync([FromBody] ApproveFreightRfqCommand command)
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

        [HttpPost("reject")]
        public async Task<IActionResult> RejectAsync([FromBody] RejectFreightRfqCommand command)
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var result = await Mediator.Send(new DeleteFreightRfqCommand(id));
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result, message = "Deleted successfully." });
        }
    }
}
