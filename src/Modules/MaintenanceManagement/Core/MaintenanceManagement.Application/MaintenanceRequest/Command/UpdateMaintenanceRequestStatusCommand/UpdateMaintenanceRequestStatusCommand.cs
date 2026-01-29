using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MediatR;

namespace MaintenanceManagement.Application.MaintenanceRequest.Command.UpdateMaintenanceRequestStatusCommand
{
    public class UpdateMaintenanceRequestStatusCommand  : IRequest<ApiResponseDTO<bool>>
    {
        public int Id { get; set; }
       // public int RequestStatusId { get; set; }
    }
}