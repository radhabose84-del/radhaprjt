using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.Complaint.Commands.CreateComplaint;
using SalesManagement.Application.Complaint.Commands.UpdateComplaint;
using SalesManagement.Application.Complaint.Commands.DeleteComplaint;
using SalesManagement.Application.Complaint.Queries.GetAllComplaint;
using SalesManagement.Application.Complaint.Queries.GetComplaintById;
using SalesManagement.Application.Complaint.Queries.GetComplaintAutoComplete;
using SalesManagement.Application.Complaint.Queries.GetCustomerInvoices;
using SalesManagement.Application.Complaint.Queries.GetInvoiceLineDetails;
using SalesManagement.Application.Complaint.Queries.GetPendingComplaint;
using SalesManagement.Application.Complaint.Queries.SearchInvoices;

namespace SalesManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class ComplaintController : ApiControllerBase
    {
        public ComplaintController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllComplaintAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllComplaintQuery
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

        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingComplaintAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetPendingComplaintQuery
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
        public async Task<IActionResult> GetComplaintByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetComplaintByIdQuery { Id = id });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetComplaintAutoCompleteAsync([FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetComplaintAutoCompleteQuery(term ?? string.Empty));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("customer-invoices")]
        public async Task<IActionResult> GetCustomerInvoicesAsync([FromQuery] int customerId)
        {
            var result = await Mediator.Send(new GetCustomerInvoicesQuery { CustomerId = customerId });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("invoice-details")]
        public async Task<IActionResult> GetInvoiceLineDetailsAsync([FromQuery] int invoiceHeaderId)
        {
            var result = await Mediator.Send(new GetInvoiceLineDetailsQuery { InvoiceHeaderId = invoiceHeaderId });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("search-invoices")]
        public async Task<IActionResult> SearchInvoicesAsync(
            [FromQuery] int partyId,
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null,
            [FromQuery] bool LastOneYear = false)
        {
            var result = await Mediator.Send(new SearchInvoicesQuery
            {
                PartyId = partyId,
                SearchTerm = SearchTerm,
                LastOneYear = LastOneYear,
                PageNumber = PageNumber,
                PageSize = PageSize
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

        [HttpPost]
        public async Task<IActionResult> CreateComplaint([FromBody] CreateComplaintCommand command)
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
        public async Task<IActionResult> UpdateComplaint([FromBody] UpdateComplaintCommand command)
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
        public async Task<IActionResult> DeleteComplaint(int id)
        {
            var result = await Mediator.Send(new DeleteComplaintCommand(id));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }
    }
}
