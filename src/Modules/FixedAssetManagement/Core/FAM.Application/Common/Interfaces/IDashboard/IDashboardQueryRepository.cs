using FAM.Application.Dashboard.CardView;
using FAM.Application.Dashboard.Common;

namespace FAM.Application.Common.Interfaces.IDashboard
{
    public interface IDashboardQueryRepository
    {
        Task<CardViewDto> GetDashboardDataAsync(DateTime fromDate, DateTime toDate);
        Task<ChartDto> GetAssetExpiredDashBoardDataAsync(DateTime fromDate, DateTime toDate);
        Task<ChartDto> GetAssetChartViewAsync(int? departmentId, DateTime fromDate, DateTime toDate);
    }
}