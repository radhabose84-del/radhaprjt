using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.WorkCenter.Command.DeleteWorkCenter
{
    public class DeleteWorkCenterCommand : IRequest<ApiResponseDTO<int>> 
    {
        public int Id { get; set; }
    }
}