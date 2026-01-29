using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MediatR;

namespace MaintenanceManagement.Application.PreventiveSchedulers.Commands.MapMachine
{
    public class MapMachineCommand : IRequest<ApiResponseDTO<bool>>
    {
        public int Id { get; set; }
        public int MachineId { get; set; }
        public DateOnly LastMaintenanceActivityDate { get; set; }
    }
}