using Contracts.Common;
using MaintenanceManagement.Application.MaintenanceRequest.Queries.GetMaintenanceRequest;
using MediatR;

namespace MaintenanceManagement.Application.MaintenanceRequest.Queries.GetMaintenanceRequestById
{
    public class GetMaintenanceRequestByIdQuery  :  IRequest<ApiResponseDTO<GetMaintenanceRequestDto>>
    {
        public int Id { get; set; }
    }
}