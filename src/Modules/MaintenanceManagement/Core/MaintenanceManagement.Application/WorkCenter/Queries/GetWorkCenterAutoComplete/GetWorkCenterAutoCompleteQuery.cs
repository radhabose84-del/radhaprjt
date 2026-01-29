using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.WorkCenter.Queries.GetWorkCenter;
using MediatR;

namespace MaintenanceManagement.Application.WorkCenter.Queries.GetWorkCenterAutoComplete
{
    public class GetWorkCenterAutoCompleteQuery : IRequest<ApiResponseDTO<List<WorkCenterAutoCompleteDto>>>
    {
        public string? SearchPattern { get; set; }
    }
}