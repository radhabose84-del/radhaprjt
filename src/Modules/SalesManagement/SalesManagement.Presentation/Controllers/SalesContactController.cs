using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.SalesContact.Commands.CreateSalesContact;
using SalesManagement.Application.SalesContact.Commands.DeleteSalesContact;
using SalesManagement.Application.SalesContact.Commands.UpdateSalesContact;
using SalesManagement.Application.SalesContact.Queries.GetAllSalesContact;
using SalesManagement.Application.SalesContact.Queries.GetSalesContactAutoComplete;
using SalesManagement.Application.SalesContact.Queries.GetSalesContactById;

namespace SalesManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class SalesContactController : ApiControllerBase
    {
        public SalesContactController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllSalesContactAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllSalesContactQuery
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
        public async Task<IActionResult> GetSalesContactByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetSalesContactByIdQuery { Id = id });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetSalesContactAutoCompleteAsync([FromQuery] string term = null!)
        {
            var result = await Mediator.Send(new GetSalesContactAutoCompleteQuery(term));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateSalesContact([FromBody] CreateSalesContactCommand command)
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
        public async Task<IActionResult> UpdateSalesContact([FromBody] UpdateSalesContactCommand command)
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
        public async Task<IActionResult> DeleteSalesContact(int id)
        {
            var result = await Mediator.Send(new DeleteSalesContactCommand(id));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Sales Contact deleted successfully." : "Sales Contact not found."
            });
        }
    }
}
