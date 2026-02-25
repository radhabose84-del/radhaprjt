using FAM.Application.Dashboard.CardView;
using FAM.Application.Dashboard.Common;

namespace FAM.Application.Common.Interfaces.IDashboard
{
    public interface IDashboardQueryRepository
    {
        Task<CardViewDto> GetDashboardDataAsync();
        Task<ChartDto> GetAssetExpiredDashBoardDataAsync();

        Task<ChartDto> GetAssetChartViewAsync(int? departmentId);

        
    }
}