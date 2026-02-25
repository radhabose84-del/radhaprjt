using Contracts.Common;
using MaintenanceManagement.Application.MiscMaster.Queries.GetMiscMaster;
using MediatR;

namespace MaintenanceManagement.Application.ActivityMaster.Queries.GetActivityType
{
    public class GetActivityTypeQuery : IRequest<ApiResponseDTO<List<GetMiscMasterDto>>>
    {
        
    }
}