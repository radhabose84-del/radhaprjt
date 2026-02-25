using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.MaintenanceRequest.Command.CreateExternalRequestWorkOrder
{
    public class CreateExternalRequestWorkOrderCommand  : IRequest<ApiResponseDTO<List<int>>>
    {
    public List<int>? Ids { get; set; } 
      

        
    }
}