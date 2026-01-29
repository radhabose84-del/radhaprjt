using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MediatR;

namespace MaintenanceManagement.Application.ShiftMasters.Queries.GetShiftMasterAutoComplete
{
    public class GetShiftMasterAutoCompleteQuery : IRequest<ApiResponseDTO<List<ShiftMasterAutoCompleteDTO>>>
    {
        public string SearchPattern { get; set; }
    }
}