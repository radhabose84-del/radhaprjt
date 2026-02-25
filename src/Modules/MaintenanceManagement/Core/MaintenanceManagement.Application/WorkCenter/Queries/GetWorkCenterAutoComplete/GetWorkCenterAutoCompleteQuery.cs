using Contracts.Common;
using MaintenanceManagement.Application.WorkCenter.Queries.GetWorkCenter;
using MediatR;

namespace MaintenanceManagement.Application.WorkCenter.Queries.GetWorkCenterAutoComplete
{
    public class GetWorkCenterAutoCompleteQuery : IRequest<ApiResponseDTO<List<WorkCenterAutoCompleteDto>>>
    {
        public string? SearchPattern { get; set; }
    }
}