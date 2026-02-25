using Contracts.Common;
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