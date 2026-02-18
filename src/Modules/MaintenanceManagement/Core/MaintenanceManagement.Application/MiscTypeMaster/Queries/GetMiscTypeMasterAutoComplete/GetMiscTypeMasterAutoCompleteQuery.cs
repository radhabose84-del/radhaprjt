using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;
using MaintenanceManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;

namespace MaintenanceManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMasterAutoComplete
{
    public class GetMiscTypeMasterAutoCompleteQuery  :  IRequest<ApiResponseDTO<List<GetMiscTypeMasterAutocompleteDto>>>
    {
          public string? SearchPattern { get; set; }
    }
}