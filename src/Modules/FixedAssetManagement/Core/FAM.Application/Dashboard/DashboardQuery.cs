using FAM.Application.Dashboard.Common;
using MediatR;

namespace FAM.Application.Dashboard
{
    public class DashboardQuery : IRequest<ChartDto>
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string? Type { get; set; }
        public int? DepartmentId { get; set; }
    }
}