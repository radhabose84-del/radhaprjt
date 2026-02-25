using Contracts.Common;
using MaintenanceManagement.Application.WorkCenter.Queries.GetWorkCenter;
using MediatR;

namespace MaintenanceManagement.Application.WorkCenter.Queries.GetWorkCenterById
{
    public class GetWorkCenterByIdQuery : IRequest<ApiResponseDTO<WorkCenterDto>>
    {
         public int Id { get; set; }
    }
}