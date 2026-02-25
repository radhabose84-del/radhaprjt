using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.MaintenanceRequest.Command.UpdateMaintenanceRequestStatusCommand
{
    public class UpdateMaintenanceRequestStatusCommand  : IRequest<ApiResponseDTO<bool>>
    {
        public int Id { get; set; }
       // public int RequestStatusId { get; set; }
    }
}