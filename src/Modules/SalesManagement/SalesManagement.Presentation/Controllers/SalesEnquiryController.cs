using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.SalesEnquiry.Commands.CreateSalesEnquiry;
using SalesManagement.Application.SalesEnquiry.Commands.UpdateSalesEnquiry;
using SalesManagement.Application.SalesEnquiry.Commands.DeleteSalesEnquiry;
using SalesManagement.Application.SalesEnquiry.Queries.GetAllSalesEnquiry;
using SalesManagement.Application.SalesEnquiry.Queries.GetSalesEnquiryAutoComplete;
using SalesManagement.Application.SalesEnquiry.Queries.GetSalesEnquiryById;

namespace SalesManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class SalesEnquiryController : ApiControllerBase
    {
        public SalesEnquiryController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllSalesEnquiryAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllSalesEnquiryQuery
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

        [HttpGet("by-name")]
        public async Task<IActionResult> GetSalesEnquiryAutoCompleteAsync([FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetSalesEnquiryAutoCompleteQuery(term));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSalesEnquiryByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetSalesEnquiryByIdQuery { Id = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateSalesEnquiry([FromBody] CreateSalesEnquiryCommand command)
        {
            var result = await Mediator.Send(command);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = true,
                message = "Sales Enquiry created successfully.",
                data = result
            });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateSalesEnquiry([FromBody] UpdateSalesEnquiryCommand command)
        {
            var result = await Mediator.Send(command);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = true,
                message = "Sales Enquiry updated successfully.",
                data = result
            });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteSalesEnquiry(int id)
        {
            var result = await Mediator.Send(new DeleteSalesEnquiryCommand(id));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Sales Enquiry deleted successfully." : "Failed to delete Sales Enquiry."
            });
        }
    }
}
