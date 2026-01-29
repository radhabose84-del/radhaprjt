using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.MiscMaster.Queries.GetMiscMaster;
using MediatR;

namespace MaintenanceManagement.Application.WorkOrder.Queries.GetWorkOrderStatus
{
    public class GetWorkOrderStatusQuery : IRequest<ApiResponseDTO<List<GetMiscMasterDto>>> 
    {
        
    }
}