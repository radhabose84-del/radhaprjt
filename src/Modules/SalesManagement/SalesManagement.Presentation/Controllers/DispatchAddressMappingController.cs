using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.DispatchAddressMapping.Commands.CreateDispatchAddressMapping;
using SalesManagement.Application.DispatchAddressMapping.Commands.DeleteDispatchAddressMapping;
using SalesManagement.Application.DispatchAddressMapping.Commands.UpdateDispatchAddressMapping;
using SalesManagement.Application.DispatchAddressMapping.Queries.GetAllDispatchAddressMapping;
using SalesManagement.Application.DispatchAddressMapping.Queries.GetDispatchAddressMappingAutoComplete;
using SalesManagement.Application.DispatchAddressMapping.Queries.GetDispatchAddressMappingById;

namespace SalesManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class DispatchAddressMappingController : ApiControllerBase
    {
        public DispatchAddressMappingController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllDispatchAddressMappingAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null,
            [FromQuery] int? PartyId = null)
        {
            var result = await Mediator.Send(new GetAllDispatchAddressMappingQuery
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                SearchTerm = SearchTerm,
                PartyId = PartyId
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
        public async Task<IActionResult> GetDispatchAddressMappingByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetDispatchAddressMappingByIdQuery { Id = id });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetDispatchAddressMappingAutoCompleteAsync([FromQuery] string term = null!)
        {
            var result = await Mediator.Send(new GetDispatchAddressMappingAutoCompleteQuery(term));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateDispatchAddressMapping([FromBody] CreateDispatchAddressMappingCommand command)
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
        public async Task<IActionResult> UpdateDispatchAddressMapping([FromBody] UpdateDispatchAddressMappingCommand command)
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
        public async Task<IActionResult> DeleteDispatchAddressMapping(int id)
        {
            var result = await Mediator.Send(new DeleteDispatchAddressMappingCommand(id));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Dispatch Address Mapping deleted successfully." : "Dispatch Address Mapping not found."
            });
        }
    }
}
