using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.PreventiveSchedulers.Commands.ReschedulePreventive
{
    public class ReshedulePreventiveCommand : IRequest<ApiResponseDTO<bool>>
    {
        public int PreventiveScheduleDetailId { get; set; }
        public DateOnly RescheduleDate { get; set; }
    }
}