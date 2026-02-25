using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.PreventiveSchedulers.Queries.GetSchedulerByDate
{
    public class GetSchedulerByDateQuery : IRequest<ApiResponseDTO<List<SchedulerByDateDto>>>
    {
        public int DepartmentId { get; set; }
    }
}