using FAM.Application.Common.Interfaces.IDashboard;
using FAM.Application.Dashboard.Common;
using MediatR;

namespace FAM.Application.Dashboard
{
    public class DashboardQueryHandler : IRequestHandler<DashboardQuery, ChartDto>
    {
        private readonly IDashboardQueryRepository _repository;
        public DashboardQueryHandler(IDashboardQueryRepository repository)
        {
            _repository = repository;
        }

        public async Task<ChartDto> Handle(DashboardQuery request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Type))
                throw new ArgumentException("Type is required. Valid values: 'assetexpirySummary', 'assetSummary'");

            return request.Type switch
            {
                "assetexpirySummary" => await _repository.GetAssetExpiredDashBoardDataAsync(request.FromDate, request.ToDate),
                "assetSummary" => await _repository.GetAssetChartViewAsync(request.DepartmentId, request.FromDate, request.ToDate),
                _ => throw new ArgumentException("Invalid type.")
            };
        }

    }
}