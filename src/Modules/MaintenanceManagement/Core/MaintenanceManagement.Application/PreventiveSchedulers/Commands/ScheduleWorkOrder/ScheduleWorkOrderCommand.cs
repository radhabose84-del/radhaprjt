using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MediatR;

namespace MaintenanceManagement.Application.PreventiveSchedulers.Commands.ScheduleWorkOrder
{
    public class ScheduleWorkOrderCommand : IRequest<ApiResponseDTO<bool>>
    {
        public int PreventiveScheduleId { get; set; }
    }
}