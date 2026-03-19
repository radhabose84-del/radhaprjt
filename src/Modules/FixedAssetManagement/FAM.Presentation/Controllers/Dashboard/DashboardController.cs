using FAM.Application.Dashboard;
using FAM.Application.Dashboard.CardView;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FAM.Presentation.Controllers.Dashboard
{
    [Route("api/fam/[controller]")]
    public class DashboardController : ApiControllerBase
    {
        public DashboardController(IMediator mediator) : base(mediator) { }

        [HttpGet("card-dashboard")]
        public async Task<IActionResult> GetCardDashboard([FromQuery] CardViewQuery request)
        {
            var data = await Mediator.Send(request);
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = true,
                message = "Card dashboard data retrieved successfully.",
                data
            });
        }

        [HttpGet("Asset-summary")]
        public async Task<IActionResult> GetAssetSummary([FromQuery] DashboardQuery request)
        {
            request.Type = "assetSummary";
            var data = await Mediator.Send(request);
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = true,
                message = "Asset summary retrieved successfully.",
                data
            });
        }

        [HttpGet("AssetExpiry-summary")]
        public async Task<IActionResult> GetAssertExpirySummary([FromQuery] DashboardQuery request)
        {
            request.Type = "assetexpirySummary";
            var data = await Mediator.Send(request);
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = true,
                message = "Asset expiry summary retrieved successfully.",
                data
            });
        }
    }
}