using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductionManagement.Application.ProductionPack.Commands.CreateProduction;
using ProductionManagement.Application.ProductionPack.Commands.UpdateProduction;
using ProductionManagement.Application.ProductionPack.Queries.GetAllProduction;
using ProductionManagement.Application.ProductionPack.Queries.GetProductionAutoComplete;
using ProductionManagement.Application.ProductionPack.Queries.GetLastEndPackNo;
using ProductionManagement.Application.ProductionPack.Queries.GetProductionById;
using ProductionManagement.Application.ProductionPack.Queries.GetProductionByPackRange;

namespace ProductionManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class ProductionPackController : ApiControllerBase
    {
        public ProductionPackController(IMediator mediator) : base(mediator) { }

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

        [HttpGet("endpackno/{productionYear}")]
        public async Task<IActionResult> GetLastEndPackNoAsync(int productionYear)
        {
            var result = await Mediator.Send(new GetLastEndPackNoQuery { ProductionYear = productionYear });

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

        [HttpGet("by-pack-range")]
        public async Task<IActionResult> GetProductionByPackRangeAsync(
            [FromQuery] int StartPackNo,
            [FromQuery] int EndPackNo)
        {
            var result = await Mediator.Send(new GetProductionByPackRangeQuery
            {
                StartPackNo = StartPackNo,
                EndPackNo   = EndPackNo
            });

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
