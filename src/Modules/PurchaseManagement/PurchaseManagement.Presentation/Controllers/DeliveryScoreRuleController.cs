using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.DeliveryScoreRule.Commands.CreateDeliveryScoreRule;
using PurchaseManagement.Application.DeliveryScoreRule.Commands.DeleteDeliveryScoreRule;
using PurchaseManagement.Application.DeliveryScoreRule.Commands.UpdateDeliveryScoreRule;
using PurchaseManagement.Application.DeliveryScoreRule.Queries.GetAllDeliveryScoreRule;
using PurchaseManagement.Application.DeliveryScoreRule.Queries.GetDeliveryScoreRuleAutoComplete;
using PurchaseManagement.Application.DeliveryScoreRule.Queries.GetDeliveryScoreRuleById;

namespace PurchaseManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class DeliveryScoreRuleController : ApiControllerBase
    {
        public DeliveryScoreRuleController(ISender mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllDeliveryScoreRuleAsync(
            [FromQuery] int PageNumber, [FromQuery] int PageSize, [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllDeliveryScoreRuleQuery
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
        public async Task<IActionResult> GetDeliveryScoreRuleByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetDeliveryScoreRuleByIdQuery { Id = id });
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetDeliveryScoreRuleAutoCompleteAsync([FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetDeliveryScoreRuleAutoCompleteQuery(term ?? string.Empty));
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result });
        }

        [HttpPost]
        public async Task<IActionResult> CreateDeliveryScoreRule([FromBody] CreateDeliveryScoreRuleCommand command)
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
        public async Task<IActionResult> UpdateDeliveryScoreRule([FromBody] UpdateDeliveryScoreRuleCommand command)
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
        public async Task<IActionResult> DeleteDeliveryScoreRule(int id)
        {
            await Mediator.Send(new DeleteDeliveryScoreRuleCommand(id));
            return Ok(new { StatusCode = StatusCodes.Status200OK, message = "Deleted successfully." });
        }
    }
}
