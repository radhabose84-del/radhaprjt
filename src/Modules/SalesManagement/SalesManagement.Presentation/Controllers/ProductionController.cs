using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.Production.Commands.CreateProduction;
using SalesManagement.Application.Production.Commands.UpdateProduction;
using SalesManagement.Application.Production.Queries.GetAllProduction;
using SalesManagement.Application.Production.Queries.GetProductionAutoComplete;
using SalesManagement.Application.Production.Queries.GetProductionById;

namespace SalesManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class ProductionController : ApiControllerBase
    {
        public ProductionController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllProductionAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllProductionQuery
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
        public async Task<IActionResult> GetProductionAutoCompleteAsync([FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetProductionAutoCompleteQuery(term));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductionByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetProductionByIdQuery { Id = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduction([FromBody] CreateProductionCommand command)
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
        public async Task<IActionResult> UpdateProduction([FromBody] UpdateProductionCommand command)
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
    }
}
