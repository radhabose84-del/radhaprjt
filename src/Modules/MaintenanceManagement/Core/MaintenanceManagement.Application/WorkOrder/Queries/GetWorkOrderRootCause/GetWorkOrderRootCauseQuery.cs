
using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.MiscMaster.Queries.GetMiscMaster;
using MediatR;

namespace MaintenanceManagement.Application.WorkOrder.Queries.GetWorkOrderRootCause
{
    public class GetWorkOrderRootCauseQuery : IRequest<ApiResponseDTO<List<GetMiscMasterDto>>> 
    {
        
    }
}