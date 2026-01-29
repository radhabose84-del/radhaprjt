using MaintenanceManagement.Application.Dashboard.CardView;
using MediatR;

namespace MaintenanceManagement.Application.Dashboard.DashboardQuery
{
    public class CardViewQuery : IRequest<CardViewDto>
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string? DepartmentId { get; set; }
        public string? MachineGroupId { get; set; }
        public string? ItemCode { get; set; }
        public string? Type { get; set; }
    }
}