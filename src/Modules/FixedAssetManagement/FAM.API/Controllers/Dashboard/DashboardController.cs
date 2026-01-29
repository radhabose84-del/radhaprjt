using FAM.Application.Common.Interfaces.IDashboard;
using FAM.Application.Dashboard;
using FAM.Application.Dashboard.CardView;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FAM.API.Controllers.Dashboard
{ 
    [ApiController]    
    [Route("[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardQueryRepository _dashboardQueryRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMediator _mediator;


        public DashboardController(IDashboardQueryRepository dashboardQueryRepository, IHttpContextAccessor httpContextAccessor, IMediator mediator)
        {
            _dashboardQueryRepository = dashboardQueryRepository;
            _httpContextAccessor = httpContextAccessor;
            _mediator = mediator;
        }

        // [HttpGet("dashboard-summary")]
        // public async Task<IActionResult> GetDashboardData()
        // {
        //     try
        //     {
        //         var result = await _dashboardQueryRepository.GetDashboardDataAsync(); // No unitId passed explicitly
        //         return Ok(new
        //         {
        //             isSuccess = true,
        //             message = "Dashboard data retrieved successfully",
        //             data = result
        //         });
        //     }
        //     catch (Exception ex)
        //     {
        //         return StatusCode(500, new
        //         {
        //             isSuccess = false,
        //             message = "Failed to retrieve dashboard data.",
        //             error = ex.Message
        //         });
        //     }
        // }


       [HttpGet("card-dashboard")]
        public async Task<IActionResult> GetCardDashboard([FromQuery] CardViewQuery request)
        {
            var data = await _mediator.Send(request);
            return Ok(data);
        }

          [HttpGet("Asset-summary")]
        public async Task<IActionResult> GetAssetSummary([FromQuery] DashboardQuery request)
        {
            request.Type = "assetSummary";
            var data = await _mediator.Send(request);
            return Ok(data);
        }
        [HttpGet("AssetExpiry-summary")]
        public async Task<IActionResult> GetAssertExpirySummary([FromQuery] DashboardQuery request)
        {
            request.Type = "assetexpirySummary";
            var data = await _mediator.Send(request);
            return Ok(data);
        }
    }
}