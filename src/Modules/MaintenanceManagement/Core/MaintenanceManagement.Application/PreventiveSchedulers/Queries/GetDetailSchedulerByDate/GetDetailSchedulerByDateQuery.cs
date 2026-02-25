using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.PreventiveSchedulers.Queries.GetDetailSchedulerByDate
{
    public class GetDetailSchedulerByDateQuery : IRequest<ApiResponseDTO<List<DetailSchedulerByDateDto>>>
    {
        public DateOnly SchedulerDate { get; set; }
        public int DepartmentId { get; set; }
    }
}