using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MediatR;
using MaintenanceManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;

namespace MaintenanceManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMasterAutoComplete
{
    public class GetMiscTypeMasterAutoCompleteQuery  :  IRequest<ApiResponseDTO<List<GetMiscTypeMasterAutocompleteDto>>>
    {
          public string? SearchPattern { get; set; }
    }
}