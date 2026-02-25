using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.PreventiveSchedulers.Commands.ScheduleWorkOrder
{
    public class ScheduleWorkOrderCommand : IRequest<ApiResponseDTO<bool>>
    {
        public int PreventiveScheduleId { get; set; }
    }
}